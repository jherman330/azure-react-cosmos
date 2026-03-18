using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todo.Api.Infrastructure.RateLimiting;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// REQ-FOUNDATION-005: registers options and <see cref="DistributedRateLimitEvaluator"/> for
/// <see cref="DistributedRateLimitingMiddleware"/> — the sole rate limit enforcement path (Redis sliding window or cache buckets).
/// </summary>
public static class RateLimitingServiceCollectionExtensions
{
    public static IServiceCollection AddDistributedRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .Validate(
                static o => o.Read.PermitLimit > 0 && o.Write.PermitLimit > 0 && o.Search.PermitLimit > 0,
                "RateLimiting Read, Write, and Search PermitLimit must be positive.")
            .ValidateOnStart();

        services.AddSingleton<DistributedRateLimitEvaluator>(sp =>
        {
            var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            var logger = sp.GetRequiredService<ILogger<DistributedRateLimitEvaluator>>();
            var redis = sp.GetService<StackExchange.Redis.IConnectionMultiplexer>();
            return new DistributedRateLimitEvaluator(cache, logger, redis);
        });

        return services;
    }
}
