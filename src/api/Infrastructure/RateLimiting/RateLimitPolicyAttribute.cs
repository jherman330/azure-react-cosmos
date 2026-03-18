namespace Todo.Api.Infrastructure.RateLimiting;

/// <summary>
/// Tier for <see cref="DistributedRateLimitingMiddleware"/> (Read / Write / Search). REQ-FOUNDATION-005.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RateLimitPolicyAttribute : Attribute
{
    public RateLimitPolicyAttribute(string tier)
    {
        Tier = tier ?? throw new ArgumentNullException(nameof(tier));
    }

    public string Tier { get; }
}
