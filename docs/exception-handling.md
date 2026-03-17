# Global Exception Handling (WO-7 / AC-FOUNDATION-007)

This document describes the API’s **global exception handling**: a single middleware that catches unhandled exceptions and returns a standardized error envelope so clients get consistent error responses across all endpoints.

## Error Envelope Format

All error responses use this JSON structure:

```json
{
  "traceId": "0HN...XXXXXXX",
  "errorCode": "CONCURRENCY_CONFLICT",
  "message": "The resource was modified by another process. Refresh and retry."
}
```

- **traceId**: Request trace identifier. Matches `HttpContext.TraceIdentifier` and correlates with Application Insights logs.
- **errorCode**: Uppercase snake_case code for programmatic handling (e.g. `NOT_FOUND`, `BAD_REQUEST`).
- **message**: User-facing message; no implementation details or stack traces are exposed in 500 responses.

## Exception → Status Code Mapping

| Exception type | HTTP status | Error code |
|----------------|-------------|------------|
| `ConcurrencyConflictException` | 412 Precondition Failed | `CONCURRENCY_CONFLICT` |
| `KeyNotFoundException` | 404 Not Found | `NOT_FOUND` |
| `ArgumentException` / `ArgumentNullException` | 400 Bad Request | `BAD_REQUEST` |
| `InvalidOperationException` | 422 Unprocessable Entity | `UNPROCESSABLE_ENTITY` |
| `TimeoutException`, `HttpRequestException` | 503 Service Unavailable | `SERVICE_UNAVAILABLE` |
| `OperationCanceledException` | 503 Service Unavailable | `SERVICE_UNAVAILABLE` |
| All other exceptions | 500 Internal Server Error | `INTERNAL_SERVER_ERROR` |

For 500 responses, the message is generic: *"An unexpected error occurred. Use the trace ID for support."*

**OperationCanceledException**: Intentionally mapped to 503 for this implementation. It can mean either (a) the client aborted the request (e.g. closed connection) or (b) an internal timeout/cancellation (e.g. `CancellationToken` fired). Treating both as 503 keeps the contract simple; you can narrow later (e.g. client disconnect → 499 or 408) if the distinction matters.

## Implementation

- **Middleware**: `Infrastructure/Middleware/ExceptionHandlingMiddleware.cs` — wraps the pipeline and catches exceptions.
- **Envelope type**: `Infrastructure/ApiErrorEnvelope.cs` — shared by the middleware and JWT auth (401/403).
- **Error codes**: `Infrastructure/ErrorCodes.cs` — centralized constants.
- **Registration**: `app.UseGlobalExceptionHandling()` is called early in `Program.cs` so it wraps all subsequent middleware and endpoints.

## Logging

Exceptions are logged at **Error** level with structured properties: `ExceptionType`, `TraceId`, `ErrorCode`, and the exception message. The full exception (including stack trace) is attached to the log entry for diagnostics.

This middleware is the only exception-handling layer in the pipeline (the app does not use `UseExceptionHandler` or `DeveloperExceptionPage`), so exceptions are not double-logged by a framework handler. If the response has already started when an exception occurs, the middleware does not write an envelope (it would be invalid); it logs a warning and rethrows so the server can close the connection cleanly.

## Authentication and Authorization Errors

401 (Unauthorized) and 403 (Forbidden) are produced by the JWT bearer middleware before the request reaches application code. They use the same envelope format and `ErrorCodes.Unauthorized` / `ErrorCodes.Forbidden` so all error responses are consistent.
