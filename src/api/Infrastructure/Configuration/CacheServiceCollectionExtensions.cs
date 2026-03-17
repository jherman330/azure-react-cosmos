using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Todo.Api.Infrastructure.Caching;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// DI registration for distributed caching (AC-FOUNDATION-010.1–010.4, 010.7).
/// Development → in-memory. Non-dev with ConnectionStrings:Redis → Redis. Non-dev without Redis → in-memory with a startup warning (not shared across instances).
/// The registered <see cref="IDistributedCache"/> is wrapped with an application-level 2s bound and graceful degradation.
/// </summary>
public static class CacheServiceCollectionExtensions
{
    private static readonly TimeSpan DefaultOperationTimeout = TimeSpan.FromSeconds(2);
    private const string RedisConnectionKey = "ConnectionStrings:Redis";

    /// <summary>
    /// Adds <see cref="IDistributedCache"/> to the container: in-memory in Development,
    /// Redis in non-Development when <see cref="RedisConnectionKey"/> is configured,
    /// otherwise in-memory with a logged warning. Wraps the implementation with an application-level operation bound and cache-aside degradation.
    /// </summary>
    public static IServiceCollection AddDistributedCache(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var redisConnection = configuration[RedisConnectionKey];
        var isDev = environment.IsDevelopment();
        var hasRedis = !string.IsNullOrWhiteSpace(redisConnection);
        var useRedis = !isDev && hasRedis;

        if (useRedis)
        {
            var timeoutMs = (int)DefaultOperationTimeout.TotalMilliseconds;
            services.AddOptions<RedisCacheOptions>()
                .Configure<IConfiguration>((opts, config) =>
                {
                    var conn = config[RedisConnectionKey];
                    opts.Configuration = conn;
                    // Apply timeouts at the Redis client level where supported (defense in depth).
                    var redisOpts = string.IsNullOrEmpty(conn)
                        ? new ConfigurationOptions()
                        : ConfigurationOptions.Parse(conn);
                    redisOpts.AbortOnConnectFail = false;
                    redisOpts.ConnectTimeout = timeoutMs;
                    redisOpts.SyncTimeout = timeoutMs;
                    opts.ConfigurationOptions = redisOpts;
                });
            services.AddKeyedSingleton<IDistributedCache>("InnerDistributedCache", (sp, _) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RedisCacheOptions>>();
                return new RedisCache(options);
            });
        }
        else
        {
            if (!isDev && !hasRedis)
            {
                services.AddSingleton<IHostedService>(sp => new CacheFallbackStartupLogger(
                    environment.EnvironmentName,
                    sp.GetRequiredService<ILogger<CacheFallbackStartupLogger>>()));
            }
            services.AddOptions<MemoryDistributedCacheOptions>();
            services.AddKeyedSingleton<IDistributedCache>("InnerDistributedCache", (sp, _) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MemoryDistributedCacheOptions>>();
                return new MemoryDistributedCache(options);
            });
        }

        var timeoutSeconds = configuration.GetValue("Cache:OperationTimeoutSeconds", (int)DefaultOperationTimeout.TotalSeconds);
        var timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));

        services.AddSingleton<IDistributedCache>(sp =>
        {
            var inner = sp.GetRequiredKeyedService<IDistributedCache>("InnerDistributedCache");
            var logger = sp.GetRequiredService<ILogger<ResilientDistributedCache>>();
            return new ResilientDistributedCache(inner, logger, timeout);
        });

        return services;
    }
}
