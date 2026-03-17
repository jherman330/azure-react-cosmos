namespace Todo.Api.Domain.Entities;

/// <summary>
/// Base contract for domain entities stored in Cosmos DB.
/// Provides id and partition key value for repository operations.
/// </summary>
public interface IDomainEntity
{
    /// <summary>Document id (maps to Cosmos DB 'id').</summary>
    string Id { get; }

    /// <summary>Partition key value for the container (path is container-specific).</summary>
    object PartitionKeyValue { get; }
}
