using Microsoft.Extensions.Logging;

namespace Todo.Api.Infrastructure.Resilience;

/// <summary>
/// Logs HTTP resilience events (retry, circuit breaker, timeout) from pipeline callbacks
/// that run outside DI scope. Must be initialized at startup (AC-FOUNDATION-011.7).
/// </summary>
internal static class ResilienceLogging
{
    private static ILogger? _logger;

    public static void Initialize(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("Todo.Api.Infrastructure.Resilience");
    }

    public static void LogRetry(int attempt, TimeSpan delay, Exception? exception)
    {
        _logger?.LogWarning(
            exception,
            "HTTP resilience: retry attempt {Attempt} after {DelayMs}ms",
            attempt,
            delay.TotalMilliseconds);
    }

    public static void LogCircuitStateChange(string state, Exception? exception = null)
    {
        if (exception != null)
            _logger?.LogWarning(exception, "HTTP resilience: circuit breaker state changed to {State}", state);
        else
            _logger?.LogInformation("HTTP resilience: circuit breaker state changed to {State}", state);
    }

    public static void LogTimeout(string timeoutKind, TimeSpan timeout)
    {
        _logger?.LogWarning(
            "HTTP resilience: {TimeoutKind} timeout after {TimeoutSeconds}s",
            timeoutKind,
            timeout.TotalSeconds);
    }
}
