using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace Todo.Api.Infrastructure.HealthChecks;

/// <summary>
/// Verifies Redis connectivity via PING (AC-FOUNDATION-004.3). Bounded by a 1s timeout.
/// </summary>
public sealed class RedisPingHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _multiplexer;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(1);

    public RedisPingHealthCheck(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pingTask = _multiplexer.GetDatabase().PingAsync();
            var delayTask = Task.Delay(Timeout, cancellationToken);
            var completed = await Task.WhenAny(pingTask, delayTask).ConfigureAwait(false);
            if (completed != pingTask)
            {
                return cancellationToken.IsCancellationRequested
                    ? HealthCheckResult.Unhealthy("Redis readiness check was cancelled.")
                    : HealthCheckResult.Unhealthy("Redis readiness check exceeded 1s timeout.");
            }

            await pingTask.ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis check failed.", ex);
        }
    }
}
