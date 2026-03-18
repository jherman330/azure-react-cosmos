using System.Globalization;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace Todo.Api.Infrastructure.RateLimiting;

/// <summary>
/// REQ-FOUNDATION-005 — sole enforcement: distributed sliding window (Redis) or sub-window buckets (<see cref="Microsoft.Extensions.Caching.Distributed.IDistributedCache"/>),
/// partition by JWT sub/oid or IP; tier from <see cref="RateLimitPolicyAttribute"/>; X-RateLimit-* headers; 429 + Retry-After.
/// </summary>
public sealed class DistributedRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DistributedRateLimitEvaluator _evaluator;
    private readonly IOptionsMonitor<RateLimitingOptions> _options;

    public DistributedRateLimitingMiddleware(
        RequestDelegate next,
        DistributedRateLimitEvaluator evaluator,
        IOptionsMonitor<RateLimitingOptions> options)
    {
        _next = next;
        _evaluator = evaluator;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!ShouldApplyRateLimit(context))
        {
            await _next(context).ConfigureAwait(false);
            return;
        }

        var tier = ResolveTier(context);
        var opt = _options.CurrentValue.GetTier(tier);
        var permitLimit = opt.PermitLimit > 0 ? opt.PermitLimit : tier switch
        {
            RateLimitTier.Write => 20,
            RateLimitTier.Search => 30,
            _ => 100,
        };
        var window = TimeSpan.FromMinutes(Math.Max(1, opt.WindowMinutes));

        var partition = ResolvePartitionKey(context);
        var decision = await _evaluator.TryAcquireAsync(tier, partition, permitLimit, window, context.RequestAborted)
            .ConfigureAwait(false);

        ApplyRateLimitHeaders(context, permitLimit, decision);

        if (!decision.Allowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = decision.RetryAfterSeconds.ToString(CultureInfo.InvariantCulture);
            return;
        }

        await _next(context).ConfigureAwait(false);
    }

    private static bool ShouldApplyRateLimit(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (string.Equals(path, "/", StringComparison.Ordinal))
            return false;
        if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
            return false;
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            return false;
        if (path.EndsWith("openapi.yaml", StringComparison.OrdinalIgnoreCase))
            return false;
        return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveTier(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
            return RateLimitTier.Read;
        var attrs = endpoint.Metadata.GetOrderedMetadata<RateLimitPolicyAttribute>().ToArray();
        var last = attrs.Length > 0 ? attrs[^1] : null;
        return last != null ? NormalizeTier(last.Tier) : RateLimitTier.Read;
    }

    private static string NormalizeTier(string tier) => tier switch
    {
        RateLimitTier.Write or "write" => RateLimitTier.Write,
        RateLimitTier.Search or "search" => RateLimitTier.Search,
        _ => RateLimitTier.Read,
    };

    private static string ResolvePartitionKey(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub")
                ?? user.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
                ?? user.FindFirstValue("oid");
            if (!string.IsNullOrEmpty(sub))
                return "u:" + sub;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return "ip:" + ip;
    }

    private static void ApplyRateLimitHeaders(HttpContext context, int limit, RateLimitDecision decision)
    {
        context.Response.Headers["X-RateLimit-Limit"] = limit.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers["X-RateLimit-Remaining"] = decision.Remaining.ToString(CultureInfo.InvariantCulture);
        context.Response.Headers["X-RateLimit-Reset"] = decision.ResetUnixSeconds.ToString(CultureInfo.InvariantCulture);
    }
}
