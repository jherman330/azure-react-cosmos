using System.Text.Json.Serialization;

namespace Todo.Api.Infrastructure;

/// <summary>
/// HTTP 400 response for FluentValidation / model binding failures (AC-FOUNDATION-008.3, 008.4).
/// Extends the standard envelope with per-field messages.
/// </summary>
public sealed class ApiValidationErrorEnvelope
{
    [JsonPropertyName("traceId")]
    public string TraceId { get; }

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    /// <summary>Field/property path (camelCase) to validation messages.</summary>
    [JsonPropertyName("errors")]
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ApiValidationErrorEnvelope(
        string traceId,
        string errorCode,
        string message,
        IReadOnlyDictionary<string, string[]> errors)
    {
        TraceId = traceId ?? string.Empty;
        ErrorCode = errorCode ?? string.Empty;
        Message = message ?? string.Empty;
        Errors = errors ?? new Dictionary<string, string[]>();
    }
}
