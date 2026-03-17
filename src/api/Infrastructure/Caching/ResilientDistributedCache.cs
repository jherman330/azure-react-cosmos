using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Todo.Api.Infrastructure.Caching;

/// <summary>
/// Wraps an <see cref="IDistributedCache"/> with an application-level operation bound (default 2s) and graceful degradation (AC-FOUNDATION-010.4, 010.7).
/// Operations are bounded by a cancellation token; whether the underlying provider (e.g. Redis) honors it is implementation-dependent—we do not claim a guaranteed hard cutoff.
/// On cancellation or exception, operations log a warning and degrade (return null / no-op) so callers can fall back to the database.
/// </summary>
public sealed class ResilientDistributedCache : IDistributedCache
{
    private readonly IDistributedCache _inner;
    private readonly ILogger<ResilientDistributedCache> _logger;
    private readonly TimeSpan _operationTimeout;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    public ResilientDistributedCache(
        IDistributedCache inner,
        ILogger<ResilientDistributedCache> logger,
        TimeSpan? operationTimeout = null)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _operationTimeout = operationTimeout ?? DefaultTimeout;
    }

    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(_operationTimeout);
        try
        {
            return await _inner.GetAsync(key, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !token.IsCancellationRequested)
        {
            _logger.LogWarning("Cache Get timed out after {Timeout}ms for key {Key}", _operationTimeout.TotalMilliseconds, key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache Get failed for key {Key}; degrading to database", key);
            return null;
        }
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(_operationTimeout);
        try
        {
            await _inner.SetAsync(key, value, options, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !token.IsCancellationRequested)
        {
            _logger.LogWarning("Cache Set timed out after {Timeout}ms for key {Key}", _operationTimeout.TotalMilliseconds, key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache Set failed for key {Key}; continuing without cache", key);
        }
    }

    public void Refresh(string key)
    {
        RefreshAsync(key).GetAwaiter().GetResult();
    }

    public async Task RefreshAsync(string key, CancellationToken token = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(_operationTimeout);
        try
        {
            await _inner.RefreshAsync(key, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !token.IsCancellationRequested)
        {
            _logger.LogWarning("Cache Refresh timed out after {Timeout}ms for key {Key}", _operationTimeout.TotalMilliseconds, key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache Refresh failed for key {Key}", key);
        }
    }

    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }

    public async Task RemoveAsync(string key, CancellationToken token = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(_operationTimeout);
        try
        {
            await _inner.RemoveAsync(key, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !token.IsCancellationRequested)
        {
            _logger.LogWarning("Cache Remove timed out after {Timeout}ms for key {Key}", _operationTimeout.TotalMilliseconds, key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache Remove failed for key {Key}", key);
        }
    }
}
