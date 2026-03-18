using Microsoft.AspNetCore.Cors.Infrastructure;
using Todo.Api.Infrastructure.Cors;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// REQ-FOUNDATION-006: CORS default behavior via <see cref="ICorsPolicyProvider"/> (built from config on first request)
/// so configuration is complete before the policy is resolved. Call <c>app.UseCors()</c> before authentication.
/// </summary>
public static class CorsServiceCollectionExtensions
{
    /// <summary>
    /// Registers CORS middleware services and a provider that applies Development vs <c>Cors:AllowedOrigins</c> rules.
    /// </summary>
    public static IServiceCollection AddConfiguredCors(this IServiceCollection services)
    {
        services.AddSingleton<ICorsPolicyProvider, ConfigurationBasedCorsPolicyProvider>();
        services.AddCors();
        return services;
    }
}
