namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// Configuration for JWT bearer authentication with Microsoft Entra ID (AC-FOUNDATION-003).
/// Binds from "Authentication:JwtBearer" in appsettings. Single-tenant only: Authority must be tenant-specific.
/// </summary>
public sealed class JwtAuthenticationOptions
{
    public const string SectionName = "Authentication:JwtBearer";

    /// <summary>
    /// Authority (identity provider URL). Must be tenant-specific, e.g. https://login.microsoftonline.com/{tenant-id}/v2.0.
    /// Do not use "common" or "organizations"; this system is explicitly single-tenant.
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Valid audience (application/client ID) for the API.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// When true, metadata (e.g. JWKS) must be retrieved over HTTPS. Set false for local development with mock identity.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;
}
