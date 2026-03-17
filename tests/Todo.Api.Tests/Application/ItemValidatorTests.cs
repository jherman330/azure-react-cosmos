using Todo.Api.Application.Validation;
using Todo.Api.Tests.Builders;
using Xunit;

namespace Todo.Api.Tests.Application;

/// <summary>
/// Example unit tests for validator (AC-FOUNDATION-012.7).
/// </summary>
[Trait(TestTraits.Category, TestTraits.FastLocal)]
[Trait(TestTraits.Category, TestTraits.FullCI)]
public sealed class ItemValidatorTests
{
    private readonly ItemValidator _sut = new();

    [Fact]
    public void Validate_Returns_Failure_When_Item_Is_Null()
    {
        var result = _sut.Validate(null!);
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("Item is required", result.Errors[0]);
    }

    [Fact]
    public void Validate_Returns_Failure_When_Id_Is_Empty()
    {
        var item = new ItemBuilder().WithId("").Build();
        var result = _sut.Validate(item);
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Contains("Id is required", result.Errors[0]);
    }

    [Fact]
    public void Validate_Returns_Failure_When_Id_Is_Whitespace()
    {
        var item = new ItemBuilder().WithId("   ").Build();
        var result = _sut.Validate(item);
        Assert.False(result.IsValid);
        Assert.Contains("Id is required", result.Errors[0]);
    }

    [Fact]
    public void Validate_Returns_Success_When_Item_Is_Valid()
    {
        var item = new ItemBuilder().WithId("valid-id").Build();
        var result = _sut.Validate(item);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
