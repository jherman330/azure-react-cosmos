namespace Todo.Api.Infrastructure.Caching;

/// <summary>
/// Cache key namespace prefixes for logical separation (AC-FOUNDATION-010.5).
/// Use lowercase with colon separators. Application cache uses "cache:*"; rate limiting uses "ratelimit:*".
/// </summary>
public static class CacheKeyNamespaces
{
    /// <summary>Application caching (e.g. cache:product:123).</summary>
    public const string Cache = "cache:";

    /// <summary>Rate limiting counters (e.g. ratelimit:ip:192.168.1.1).</summary>
    public const string RateLimit = "ratelimit:";
}
