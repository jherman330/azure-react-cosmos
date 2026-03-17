using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Todo.Api.Infrastructure.Caching;

/// <summary>
/// Extension methods for <see cref="IDistributedCache"/> using System.Text.Json serialization (AC-FOUNDATION-010.6).
/// Use with cache keys that follow <see cref="CacheKeyNamespaces"/> (e.g. cache:product:123).
/// </summary>
public static class DistributedCacheJsonExtensions
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Gets a value and deserializes it to <typeparamref name="T"/> using System.Text.Json, or null if missing or on error.
    /// </summary>
    public static async Task<T?> GetJsonAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default) where T : class
    {
        var bytes = await cache.GetAsync(key, token).ConfigureAwait(false);
        if (bytes is null || bytes.Length == 0)
            return null;
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, DefaultOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Serializes <paramref name="value"/> with System.Text.Json and stores it with the given options.
    /// </summary>
    public static async Task SetJsonAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default) where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, DefaultOptions);
        await cache.SetAsync(key, bytes, options, token).ConfigureAwait(false);
    }
}
