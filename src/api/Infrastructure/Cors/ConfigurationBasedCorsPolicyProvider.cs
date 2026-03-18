using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Todo.Api.Infrastructure.Cors;

/// <summary>
/// Single default CORS policy for <c>UseCors()</c>. See lazy field below for why resolution is deferred.
/// </summary>
public sealed class ConfigurationBasedCorsPolicyProvider : ICorsPolicyProvider
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Policy is built lazily so <see cref="IConfiguration"/> is complete first — e.g. integration tests add
    /// sources via <c>ConfigureAppConfiguration</c> after the host starts building. After the first build,
    /// the same <see cref="CorsPolicy"/> instance is reused (no per-request rebuild).
    /// </summary>
    private readonly Lazy<CorsPolicy> _policy;

    public ConfigurationBasedCorsPolicyProvider(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
        _policy = new Lazy<CorsPolicy>(
            () => BuildPolicy(CorsPolicySettings.Resolve(_configuration, _environment)),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Task<CorsPolicy?> GetPolicyAsync(HttpContext context, string? policyName) =>
        Task.FromResult<CorsPolicy?>(_policy.Value);

    private static CorsPolicy BuildPolicy(CorsPolicySettings settings)
    {
        if (settings.UsePermissiveDevelopmentPolicy)
        {
            return new CorsPolicyBuilder()
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetPreflightMaxAge(TimeSpan.FromHours(1))
                .Build();
        }

        var builder = new CorsPolicyBuilder();
        if (settings.RestrictedOrigins.Length > 0)
            builder.WithOrigins(settings.RestrictedOrigins);
        else
            builder.SetIsOriginAllowed(_ => false);

        return builder
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
            .WithHeaders(
                "Authorization",
                "Content-Type",
                "Accept",
                "X-Correlation-Id",
                "X-Request-Id")
            .SetPreflightMaxAge(TimeSpan.FromHours(1))
            .Build();
    }
}
