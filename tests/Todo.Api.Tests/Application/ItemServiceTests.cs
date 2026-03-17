using Moq;
using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Todo.Api.Tests.Builders;
using Xunit;

namespace Todo.Api.Tests.Application;

/// <summary>
/// Example unit tests for application service with mocked IRepository (AC-FOUNDATION-012.6).
/// </summary>
[Trait(TestTraits.Category, TestTraits.FastLocal)]
[Trait(TestTraits.Category, TestTraits.FullCI)]
public sealed class ItemServiceTests
{
    private readonly Mock<IRepository<Item>> _repositoryMock;
    private readonly ItemService _sut;

    public ItemServiceTests()
    {
        _repositoryMock = new Mock<IRepository<Item>>();
        _sut = new ItemService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Id_Is_Null()
    {
        var result = await _sut.GetByIdAsync(null!);
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Id_Is_Whitespace()
    {
        var result = await _sut.GetByIdAsync("   ");
        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_Calls_Repository_With_Id_As_PartitionKey()
    {
        var item = new ItemBuilder().WithId("id-1").Build();
        _repositoryMock
            .Setup(r => r.GetByIdAsync("id-1", "id-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        var result = await _sut.GetByIdAsync("id-1");

        Assert.Same(item, result);
        _repositoryMock.Verify(r => r.GetByIdAsync("id-1", "id-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Repository_Returns_Null()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync("missing", "missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Item?)null);

        var result = await _sut.GetByIdAsync("missing");

        Assert.Null(result);
    }
}
