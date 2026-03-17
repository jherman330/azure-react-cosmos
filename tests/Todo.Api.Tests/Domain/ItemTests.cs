using Todo.Api.Domain.Entities;
using Todo.Api.Tests.Builders;
using Xunit;

namespace Todo.Api.Tests.Domain;

/// <summary>
/// Example unit tests for domain entity (AC-FOUNDATION-012.5).
/// </summary>
[Trait(TestTraits.Category, TestTraits.FastLocal)]
[Trait(TestTraits.Category, TestTraits.FullCI)]
public sealed class ItemTests
{
    [Fact]
    public void Item_Implements_IDomainEntity()
    {
        var item = new ItemBuilder().WithId("x").Build();
        Assert.Equal("x", item.Id);
        Assert.Equal("x", item.PartitionKeyValue);
    }

    [Fact]
    public void Item_Implements_IAuditableEntity_and_IConcurrencyEntity()
    {
        var created = DateTimeOffset.UtcNow.AddMinutes(-1);
        var updated = DateTimeOffset.UtcNow;
        var item = new ItemBuilder()
            .WithId("audit-1")
            .WithAudit(created, updated)
            .WithEtag("etag-1")
            .Build();
        Assert.Equal(created, item.CreatedAt);
        Assert.Equal(updated, item.UpdatedAt);
        Assert.Equal("etag-1", item.Etag);
    }

    [Fact]
    public void Item_PartitionKeyValue_Matches_Id()
    {
        var item = new ItemBuilder().WithId("pk-test").Build();
        Assert.Same(item.PartitionKeyValue, item.PartitionKeyValue);
        Assert.Equal(item.Id, item.PartitionKeyValue.ToString());
    }
}
