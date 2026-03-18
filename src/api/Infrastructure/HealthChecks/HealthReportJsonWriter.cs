using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Todo.Api.Infrastructure.HealthChecks;

/// <summary>
/// JSON writers for liveness (status + duration) and readiness (status + checks + duration) per AC-FOUNDATION-004.6.
/// </summary>
public static class HealthReportJsonWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>Liveness: no per-dependency entries (AC-FOUNDATION-004.2).</summary>
    public static Task WriteLivenessAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        var body = new
        {
            status = report.Status.ToString(),
            duration = FormatDuration(report.TotalDuration)
        };
        return httpContext.Response.WriteAsJsonAsync(body, JsonOptions);
    }

    /// <summary>Readiness: includes <c>checks</c> map (AC-FOUNDATION-004.6).</summary>
    public static Task WriteReadinessAsync(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = "application/json; charset=utf-8";
        var checks = report.Entries.ToDictionary(
            static e => e.Key,
            static e => new HealthCheckEntryDto(
                e.Value.Status.ToString(),
                string.IsNullOrEmpty(e.Value.Description) ? null : e.Value.Description,
                e.Value.Duration));
        var body = new ReadinessResponseDto(
            report.Status.ToString(),
            FormatDuration(report.TotalDuration),
            checks);
        return httpContext.Response.WriteAsJsonAsync(body, JsonOptions);
    }

    private static string FormatDuration(TimeSpan d) =>
        d.ToString(@"hh\:mm\:ss\.fffffff", CultureInfo.InvariantCulture);

    private sealed record HealthCheckEntryDto(string Status, string? Description, TimeSpan Duration);

    private sealed record ReadinessResponseDto(string Status, string Duration, Dictionary<string, HealthCheckEntryDto> Checks);
}
