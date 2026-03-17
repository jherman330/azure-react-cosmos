using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Todo.Api.Infrastructure.Resilience;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// HTTP resilience registration for outbound calls (AC-FOUNDATION-011).
/// Uses the .NET 8+ standard resilience handler: retry (exponential backoff + jitter),
/// circuit breaker, and timeout. Configuration is under appsettings "Resilience:Http".
/// Applies to all HttpClients registered via <see cref="IHttpClientFactory"/> unless overridden.
/// </summary>
public static class ResilienceServiceCollectionExtensions
{
    /// <summary>Configuration section for HTTP standard resilience options.</summary>
    public const string ResilienceHttpSectionName = "Resilience:Http";

    /// <summary>
    /// Registers the standard HTTP resilience handler for all HttpClients created by
    /// <see cref="IHttpClientFactory"/>. Binds options from "Resilience:Http" and wires
    /// logging for retry attempts, circuit breaker state changes, and timeouts.
    /// Call once at startup; then use <see cref="AddHttpClient"/> (typed or named) as usual.
    /// </summary>
    public static IServiceCollection AddHttpResilience(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(ResilienceHttpSectionName);

        services.ConfigureHttpClientDefaults(builder =>
        {
            builder.AddStandardResilienceHandler(options =>
            {
                if (section.Exists())
                    section.Bind(options);
                WireResilienceLogging(options);
            });
        });

        return services;
    }

    /// <summary>
    /// Wires logging for retry, circuit breaker, and timeout events (AC-FOUNDATION-011.7).
    /// </summary>
    private static void WireResilienceLogging(HttpStandardResilienceOptions options)
    {
        options.Retry.OnRetry = static args =>
        {
            ResilienceLogging.LogRetry(args.AttemptNumber + 1, args.RetryDelay, args.Outcome.Exception);
            return default;
        };

        options.CircuitBreaker.OnOpened = static args =>
        {
            ResilienceLogging.LogCircuitStateChange("Open", args.Outcome.Exception);
            return default;
        };
        options.CircuitBreaker.OnClosed = static _ =>
        {
            ResilienceLogging.LogCircuitStateChange("Closed");
            return default;
        };
        options.CircuitBreaker.OnHalfOpened = static _ =>
        {
            ResilienceLogging.LogCircuitStateChange("HalfOpen");
            return default;
        };

        options.AttemptTimeout.OnTimeout = static args =>
        {
            ResilienceLogging.LogTimeout("Attempt", args.Timeout);
            return default;
        };
        options.TotalRequestTimeout.OnTimeout = static args =>
        {
            ResilienceLogging.LogTimeout("TotalRequest", args.Timeout);
            return default;
        };
    }

    /// <summary>
    /// Registers a typed HTTP client for outbound external calls. Resilience (retry, circuit breaker,
    /// timeout) is applied automatically via <see cref="AddHttpResilience"/>. Use this for external
    /// service integrations only.
    /// </summary>
    /// <typeparam name="TClient">Typed client interface or class.</typeparam>
    /// <typeparam name="TImplementation">Implementation that receives <see cref="HttpClient"/>.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="configureClient">Optional client configuration (e.g. BaseAddress).</param>
    public static IHttpClientBuilder AddResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        Action<HttpClient>? configureClient = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        var builder = services.AddHttpClient<TClient, TImplementation>(client => configureClient?.Invoke(client));
        return builder;
    }
}
