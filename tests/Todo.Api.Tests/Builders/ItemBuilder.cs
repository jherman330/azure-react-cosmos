using Todo.Api.Domain.Entities;

namespace Todo.Api.Tests.Builders;

/// <summary>
/// Test data builder for Item entity. Supports reusable test data (AC-FOUNDATION-012).
/// </summary>
public sealed class ItemBuilder
{
    private string _id = "item-1";
    private DateTimeOffset? _createdAt;
    private DateTimeOffset? _updatedAt;
    private string? _etag;

    public ItemBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ItemBuilder WithAudit(DateTimeOffset? createdAt, DateTimeOffset? updatedAt)
    {
        _createdAt = createdAt;
        _updatedAt = updatedAt;
        return this;
    }

    public ItemBuilder WithEtag(string? etag)
    {
        _etag = etag;
        return this;
    }

    public Item Build()
    {
        return new Item
        {
            Id = _id,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            Etag = _etag
        };
    }
}
