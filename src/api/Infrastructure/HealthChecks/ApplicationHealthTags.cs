namespace Todo.Api.Infrastructure.HealthChecks;

/// <summary>
/// Health check tags: liveness probes must not depend on external systems; readiness includes dependency checks (WO-4 / AC-FOUNDATION-004).
/// </summary>
public static class ApplicationHealthTags
{
    public const string Live = "live";
    public const string Ready = "ready";
}
