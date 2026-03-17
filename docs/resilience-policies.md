# HTTP Resilience Policies (WO-11 / AC-FOUNDATION-011)

This document describes the application’s **outbound HTTP resilience** setup: retry, circuit breaker, and timeout for calls to external services. It does **not** cover inbound request handling or other resilience patterns (bulkhead, hedging, fallback).

## Scope: Outbound Calls Only

Resilience is applied only to **outbound** HTTP calls made by the API (e.g. calling another API, a third-party service, or an external data source). Inbound requests to this API are not wrapped by these policies.

## Stack and Registration

- **Library**: `Microsoft.Extensions.Http.Resilience` ( .NET 8+ standard HTTP resilience, built on Polly).
- **Registration**: A single call to `AddHttpResilience(configuration)` in the composition root applies the **standard resilience handler** to **all** `HttpClient` instances created via `IHttpClientFactory`. No per-client handler registration is required unless you need different options for a specific client.

## Policies in Use

1. **Retry** – Exponential backoff with jitter. Handles transient failures (e.g. 408, 429, 5xx, `HttpRequestException`, timeouts). Configurable max attempts and base delay.
2. **Circuit breaker** – Reduces load on a failing dependency by opening after a failure ratio over a sampling window, then allowing probes (half-open) before closing again.
3. **Timeout** – Two levels:
   - **Attempt timeout**: per-try time limit.
   - **Total request timeout**: overall time for the request including retries.

All policy parameters are **externalized** to configuration (see below).

## Configuration

Configuration lives under **`Resilience:Http`** in `appsettings.json` (or environment-specific files). The section binds to `HttpStandardResilienceOptions`.

Example structure:

```json
{
  "Resilience": {
    "Http": {
      "Retry": {
        "MaxRetryAttempts": 3,
        "BackoffType": "Exponential",
        "UseJitter": true,
        "Delay": "00:00:02"
      },
      "CircuitBreaker": {
        "FailureRatio": 0.1,
        "MinimumThroughput": 10,
        "SamplingDuration": "00:00:30",
        "BreakDuration": "00:00:05"
      },
      "TotalRequestTimeout": { "Timeout": "00:00:30" },
      "AttemptTimeout": { "Timeout": "00:00:10" }
    }
  }
}
```

If the section is missing, library defaults are used and resilience is still applied with default values.

## Applying Resilience to Typed Clients

Because resilience is applied via `ConfigureHttpClientDefaults`, **any** `HttpClient` created through `IHttpClientFactory` (typed or named) gets the same retry, circuit breaker, and timeout behavior by default.

**Typed client example:**

```csharp
// In your DI registration (e.g. extension or Program.cs):
services.AddHttpClient<IMyExternalService, MyExternalService>(client =>
{
    client.BaseAddress = new Uri("https://external-api.example.com");
});

// Or use the convenience extension:
services.AddResilientHttpClient<IMyExternalService, MyExternalService>(client =>
{
    client.BaseAddress = new Uri("https://external-api.example.com");
});
```

Both forms use the same resilience pipeline; `AddResilientHttpClient` is a thin wrapper that only adds the typed client and optional `HttpClient` configuration.

**Named client:** `AddHttpClient("MyClient", client => { ... })` also gets the standard resilience handler automatically.

## When to Use Retry vs. Not

- **Use retry** for **idempotent** or **safe-to-retry** outbound calls (e.g. GET, or idempotent PUT with the same key). The default handler retries on transient errors and timeouts.
- **Avoid retry** (or restrict it) for **non-idempotent** operations (e.g. POST that creates a new resource, or payment calls) unless the downstream API is explicitly idempotent (e.g. by idempotency key). The library supports disabling retry for specific HTTP methods if you configure a custom handler for that client.

For most read-only or idempotent external integrations, the default “retry with exponential backoff + jitter” is appropriate.

## Logging

The following events are logged (after `ResilienceLogging.Initialize` is called at startup):

- **Retry**: each retry attempt (attempt number, delay, optional exception).
- **Circuit breaker**: state changes (Open, HalfOpen, Closed), with exception when the circuit opens.
- **Timeout**: attempt timeout and total request timeout (when they occur).

Logger category: `Todo.Api.Infrastructure.Resilience`. Do not rely on “automatic” logging from the library alone; the application explicitly wires these callbacks so that retries, circuit breaker transitions, and timeouts are always logged in a consistent format.

## Acceptance Criteria Mapping (AC-FOUNDATION-011)

| Criterion | Implementation |
|-----------|----------------|
| AC-FOUNDATION-011.1 | Resilience (standard handler) is registered in DI and applied via `ConfigureHttpClientDefaults` to all factory-created HttpClients. |
| AC-FOUNDATION-011.2 | Retry uses exponential backoff with jitter (library default; configurable via `Resilience:Http:Retry`). |
| AC-FOUNDATION-011.3 | Circuit breaker transitions Closed → Open → HalfOpen → Closed per library behavior; state changes are logged. |
| AC-FOUNDATION-011.4 | Timeout policy: `AttemptTimeout` and `TotalRequestTimeout` enforce per-attempt and total request time boundaries. |
| AC-FOUNDATION-011.5 | Policies are applied to typed/named HTTP clients automatically via `IHttpClientFactory` defaults. |
| AC-FOUNDATION-011.6 | Policy configuration is externalized to `Resilience:Http` in appsettings. |
| AC-FOUNDATION-011.7 | Retry attempts, circuit breaker state changes, and timeout events are logged via wired callbacks. |
| AC-FOUNDATION-011.8 | This document explains how to apply policies to external service integrations and when to use retry. |

*Note: The original criteria refer to “Polly”; the implementation uses the modern `Microsoft.Extensions.Http.Resilience` stack (Polly-based). The intent (retry, circuit breaker, timeout, config, logging, documentation) is unchanged.*
