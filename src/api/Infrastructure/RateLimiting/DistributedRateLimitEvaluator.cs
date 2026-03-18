using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Todo.Api.Infrastructure.RateLimiting;

/// <summary>
/// Sliding-window rate limit using Redis sorted sets when <see cref="IConnectionMultiplexer"/> is available (AC-FOUNDATION-005.9),
/// otherwise sub-window counters in <see cref="IDistributedCache"/> (in-memory in Development).
/// </summary>
public sealed class DistributedRateLimitEvaluator
{
    private const string KeyPrefix = "rl:sw:v1:";
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<DistributedRateLimitEvaluator> _logger;

    private static readonly string LuaAcquire = """
        local key = KEYS[1]
        local now = tonumber(ARGV[1])
        local windowMs = tonumber(ARGV[2])
        local limit = tonumber(ARGV[3])
        local member = ARGV[4]
        redis.call('ZREMRANGEBYSCORE', key, '-inf', now - windowMs)
        local n = redis.call('ZCARD', key)
        if n >= limit then
          local oldest = redis.call('ZRANGE', key, 0, 0, 'WITHSCORES')
          local retrySec = 1
          if oldest[2] then
            local oldestMs = tonumber(oldest[2])
            retrySec = math.max(1, math.ceil((oldestMs + windowMs - now) / 1000))
          end
          return { 0, n, retrySec }
        end
        redis.call('ZADD', key, now, member)
        redis.call('PEXPIRE', key, windowMs + 2000)
        return { 1, n + 1, 0 }
        """;

    public DistributedRateLimitEvaluator(
        IDistributedCache cache,
        ILogger<DistributedRateLimitEvaluator> logger,
        IConnectionMultiplexer? redis = null)
    {
        _cache = cache;
        _logger = logger;
        _redis = redis;
    }

    public async Task<RateLimitDecision> TryAcquireAsync(
        string tier,
        string partitionKey,
        int permitLimit,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        if (permitLimit <= 0)
            permitLimit = 1;

        var safeKey = SanitizePartition(partitionKey);
        var redisKey = KeyPrefix + tier + ":" + safeKey;
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var windowMs = (long)window.TotalMilliseconds;
        if (windowMs < 1000)
            windowMs = 1000;

        var resetUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)Math.Ceiling(window.TotalSeconds);

        if (_redis is { IsConnected: true })
        {
            try
            {
                var db = _redis.GetDatabase();
                var member = nowMs.ToString(CultureInfo.InvariantCulture) + ":" + RandomNumberGenerator.GetInt32(int.MaxValue);
                var result = (RedisValue[]?)await db.ScriptEvaluateAsync(
                    LuaAcquire,
                    keys: new RedisKey[] { redisKey },
                    values: new RedisValue[]
                    {
                        nowMs,
                        windowMs,
                        permitLimit,
                        member,
                    }).ConfigureAwait(false);

                if (result is { Length: >= 2 })
                {
                    var allowed = (int)result[0] == 1;
                    var count = (int)result[1];
                    var retrySec = result.Length >= 3 ? (int)result[2] : 1;
                    if (!allowed)
                        return new RateLimitDecision(false, 0, resetUnix, retrySec);

                    var remaining = Math.Max(0, permitLimit - count);
                    return new RateLimitDecision(true, remaining, resetUnix, 0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis rate limit failed for {Tier}; falling back to distributed cache", tier);
            }
        }

        return await TryAcquireCacheBucketsAsync(redisKey, permitLimit, window, resetUnix, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<RateLimitDecision> TryAcquireCacheBucketsAsync(
        string logicalKey,
        int permitLimit,
        TimeSpan window,
        long resetUnix,
        CancellationToken cancellationToken)
    {
        const int bucketSeconds = 10;
        var numBuckets = Math.Max(1, (int)Math.Ceiling(window.TotalSeconds / bucketSeconds));
        var now = DateTimeOffset.UtcNow;
        var currentBucket = now.ToUnixTimeSeconds() / bucketSeconds;
        var prefix = logicalKey + ":b:";

        var total = 0;
        for (var i = 0; i < numBuckets; i++)
        {
            var key = prefix + (currentBucket - i);
            var bytes = await _cache.GetAsync(key, cancellationToken).ConfigureAwait(false);
            if (bytes is { Length: > 0 } && int.TryParse(Encoding.UTF8.GetString(bytes), out var c))
                total += c;
        }

        if (total >= permitLimit)
        {
            return new RateLimitDecision(false, 0, resetUnix, Math.Max(1, bucketSeconds));
        }

        var incKey = prefix + currentBucket;
        var prev = 0;
        var prevBytes = await _cache.GetAsync(incKey, cancellationToken).ConfigureAwait(false);
        if (prevBytes is { Length: > 0 })
            int.TryParse(Encoding.UTF8.GetString(prevBytes), out prev);

        var next = prev + 1;
        await _cache.SetAsync(
            incKey,
            Encoding.UTF8.GetBytes(next.ToString(CultureInfo.InvariantCulture)),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = window + TimeSpan.FromSeconds(bucketSeconds * 2),
            },
            cancellationToken).ConfigureAwait(false);

        var remaining = Math.Max(0, permitLimit - total - 1);
        return new RateLimitDecision(true, remaining, resetUnix, 0);
    }

    private static string SanitizePartition(string partitionKey)
    {
        if (string.IsNullOrEmpty(partitionKey))
            return "unknown";
        var sb = new StringBuilder(partitionKey.Length);
        foreach (var ch in partitionKey)
        {
            if (char.IsAsciiLetterOrDigit(ch) || ch is '_' or '-' or '.')
                sb.Append(ch);
            else
                sb.Append('_');
        }
        return sb.ToString();
    }
}
