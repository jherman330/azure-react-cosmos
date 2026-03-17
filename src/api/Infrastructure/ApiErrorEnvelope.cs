using System.Text.Json.Serialization;

namespace Todo.Api.Infrastructure;

/// <summary>
/// Standardized error response envelope for all API error responses (AC-FOUNDATION-007.2, 007.7).
/// traceId correlates with Application Insights logs; errorCode and message are user-facing.
/// </summary>
public sealed class ApiErrorEnvelope
{
    [JsonPropertyName("traceId")]
    public string TraceId { get; }

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    public ApiErrorEnvelope(string traceId, string errorCode, string message)
    {
        TraceId = traceId ?? string.Empty;
        ErrorCode = errorCode ?? string.Empty;
        Message = message ?? string.Empty;
    }
}
