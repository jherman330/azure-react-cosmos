namespace Todo.Api.Infrastructure.RateLimiting;

/// <summary>Configuration under <c>RateLimiting</c> (REQ-FOUNDATION-005.10).</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public TierOptions Read { get; set; } = new() { PermitLimit = 100, WindowMinutes = 1 };
    public TierOptions Write { get; set; } = new() { PermitLimit = 20, WindowMinutes = 1 };
    public TierOptions Search { get; set; } = new() { PermitLimit = 30, WindowMinutes = 1 };

    public TierOptions GetTier(string tier) => tier switch
    {
        RateLimitTier.Write => Write,
        RateLimitTier.Search => Search,
        _ => Read,
    };

    public sealed class TierOptions
    {
        /// <summary>Maximum requests per sliding window (AC-FOUNDATION-005.4–005.6).</summary>
        public int PermitLimit { get; set; }

        /// <summary>Sliding window length in minutes.</summary>
        public int WindowMinutes { get; set; } = 1;
    }
}
