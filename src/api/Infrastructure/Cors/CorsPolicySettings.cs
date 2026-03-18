namespace Todo.Api.Infrastructure.Cors;

/// <summary>
/// Parsed CORS configuration at startup (AC-FOUNDATION-006). Used for the default CORS policy and startup diagnostics.
/// </summary>
public sealed class CorsPolicySettings
{
    private CorsPolicySettings(
        bool usePermissiveDevelopmentPolicy,
        string[] restrictedOrigins,
        bool wildcardOutsideDevelopment,
        IReadOnlyList<string> malformedOriginEntries)
    {
        UsePermissiveDevelopmentPolicy = usePermissiveDevelopmentPolicy;
        RestrictedOrigins = restrictedOrigins;
        WildcardOutsideDevelopment = wildcardOutsideDevelopment;
        MalformedOriginEntries = malformedOriginEntries;
    }

    /// <summary>True when <see cref="IHostEnvironment.IsDevelopment"/> — <c>Cors:AllowedOrigins</c> is not applied.</summary>
    public bool UsePermissiveDevelopmentPolicy { get; }

    /// <summary>Origins for the restricted policy (framework WithOrigins). Empty means fail-closed for browser CORS.</summary>
    public string[] RestrictedOrigins { get; }

    /// <summary>True when not Development and <c>Cors:AllowedOrigins</c> is exactly <c>*</c>.</summary>
    public bool WildcardOutsideDevelopment { get; }

    public IReadOnlyList<string> MalformedOriginEntries { get; }

    public static CorsPolicySettings Resolve(IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            return new CorsPolicySettings(
                usePermissiveDevelopmentPolicy: true,
                restrictedOrigins: Array.Empty<string>(),
                wildcardOutsideDevelopment: false,
                malformedOriginEntries: Array.Empty<string>());
        }

        var raw = configuration["Cors:AllowedOrigins"] ?? string.Empty;
        var trimmed = raw.Trim();
        var malformed = new List<string>();
        var origins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.Equals(trimmed, "*", StringComparison.Ordinal))
        {
            return new CorsPolicySettings(
                usePermissiveDevelopmentPolicy: false,
                restrictedOrigins: Array.Empty<string>(),
                wildcardOutsideDevelopment: true,
                malformedOriginEntries: Array.Empty<string>());
        }

        foreach (var entry in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (Uri.TryCreate(entry, UriKind.Absolute, out var uri) &&
                (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
            {
                origins.Add(entry.TrimEnd('/'));
            }
            else
            {
                malformed.Add(entry);
            }
        }

        return new CorsPolicySettings(
            usePermissiveDevelopmentPolicy: false,
            restrictedOrigins: origins.ToArray(),
            wildcardOutsideDevelopment: false,
            malformedOriginEntries: malformed);
    }

    /// <summary>
    /// Logs configuration issues this type can determine from config parsing (wildcard outside Development, bad entries, empty allowlist).
    /// </summary>
    public void LogStartupIssues(ILogger logger)
    {
        if (WildcardOutsideDevelopment)
        {
            logger.LogCritical(
                "CORS: Wildcard Cors:AllowedOrigins is not allowed outside Development. " +
                "Browser cross-origin requests will not receive Access-Control-Allow-Origin.");
        }

        foreach (var entry in MalformedOriginEntries)
        {
            logger.LogWarning("CORS: Skipping invalid AllowedOrigins entry: {Entry}", entry);
        }

        if (!UsePermissiveDevelopmentPolicy
            && RestrictedOrigins.Length == 0
            && !WildcardOutsideDevelopment)
        {
            logger.LogWarning(
                "CORS: No valid AllowedOrigins configured outside Development. " +
                "Browser cross-origin requests will not receive Access-Control-Allow-Origin.");
        }
    }
}
