using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Todo.Api.Infrastructure.HealthChecks;

/// <summary>
/// Verifies Cosmos DB account/database reachability (AC-FOUNDATION-004.3). Bounded by a 2s timeout.
/// </summary>
public sealed class CosmosDbHealthCheck : IHealthCheck
{
    private readonly CosmosClient _client;
    private readonly IConfiguration _configuration;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    public CosmosDbHealthCheck(CosmosClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var databaseId = _configuration["AZURE_COSMOS_DATABASE_NAME"] ?? "App";
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Timeout);
            await _client.GetDatabase(databaseId).ReadAsync(cancellationToken: cts.Token).ConfigureAwait(false);
            return HealthCheckResult.Healthy();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Cosmos DB readiness check exceeded 2s timeout.");
        }
        catch (CosmosException ex)
        {
            return HealthCheckResult.Unhealthy($"Cosmos DB check failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cosmos DB check failed.", ex);
        }
    }
}
