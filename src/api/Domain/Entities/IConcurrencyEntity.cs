namespace Todo.Api.Domain.Entities;

/// <summary>
/// Entity that supports optimistic concurrency via ETag (AC-FOUNDATION-002.4).
/// Cosmos DB sets this on read; repository uses it for conditional update/delete.
/// </summary>
public interface IConcurrencyEntity
{
    /// <summary>ETag from the last read; used for If-Match on update/delete.</summary>
    string? Etag { get; set; }
}
