namespace Todo.Api.Infrastructure.RateLimiting;

/// <summary>Rate limit policy tier (REQ-FOUNDATION-005).</summary>
public static class RateLimitTier
{
    public const string Read = "Read";
    public const string Write = "Write";
    public const string Search = "Search";
}
