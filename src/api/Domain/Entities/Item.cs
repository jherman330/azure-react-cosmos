namespace Todo.Api.Domain.Entities;

/// <summary>
/// Minimal entity for the default Cosmos "Items" container (partition key /id).
/// Used to satisfy AC-FOUNDATION-002.7: at least one IRepository{T} registered and injectable end-to-end.
/// Replace or extend with domain-specific entities (e.g. TodoItem) in later work orders.
/// </summary>
public sealed class Item : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public object PartitionKeyValue => Id;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }
}
