using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Todo.Api.Infrastructure.HealthChecks;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// Registers ASP.NET Core health checks for liveness and readiness (WO-4 / AC-FOUNDATION-004).
/// </summary>
public static class HealthServiceCollectionExtensions
{
    /// <summary>
    /// Adds health checks: liveness (no external deps); readiness (Cosmos when configured, Redis when used in non-Development).
    /// </summary>
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var redisConnection = configuration["ConnectionStrings:Redis"];
        var isDev = environment.IsDevelopment();
        var hasRedis = !string.IsNullOrWhiteSpace(redisConnection);
        var useRedis = !isDev && hasRedis;
        var hasCosmos = !string.IsNullOrWhiteSpace(configuration["AZURE_COSMOS_ENDPOINT"]);

        var builder = services.AddHealthChecks()
            .AddCheck(
                "liveness",
                () => HealthCheckResult.Healthy(),
                tags: new[] { ApplicationHealthTags.Live });

        if (hasCosmos)
        {
            builder.AddCheck<CosmosDbHealthCheck>(
                "cosmos",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { ApplicationHealthTags.Ready },
                timeout: TimeSpan.FromSeconds(2));
        }

        if (useRedis)
        {
            builder.AddCheck<RedisPingHealthCheck>(
                "redis",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { ApplicationHealthTags.Ready },
                timeout: TimeSpan.FromSeconds(1));
        }

        if (!hasCosmos && !useRedis)
        {
            builder.AddCheck(
                "dependencies",
                () => HealthCheckResult.Healthy("Cosmos and Redis are not configured; readiness assumes no external deps for this environment."),
                tags: new[] { ApplicationHealthTags.Ready });
        }

        return services;
    }
}
