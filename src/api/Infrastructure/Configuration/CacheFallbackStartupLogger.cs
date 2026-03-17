namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// Logs a one-time warning at startup when running in non-Development without ConnectionStrings:Redis (in-memory fallback).
/// </summary>
internal sealed class CacheFallbackStartupLogger : IHostedService
{
    private readonly string _environmentName;
    private readonly ILogger<CacheFallbackStartupLogger> _logger;

    public CacheFallbackStartupLogger(string environmentName, ILogger<CacheFallbackStartupLogger> logger)
    {
        _environmentName = environmentName;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "ConnectionStrings:Redis is not set in {Environment}. Using in-memory cache; data is not shared across instances.",
            _environmentName);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
