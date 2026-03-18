using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Validation;

/// <summary>
/// Validates <see cref="Item"/> outside the FluentValidation HTTP pipeline. Not scanned for DI/controller validation—do not replace with <c>AbstractValidator&lt;Item&gt;</c> for transport.
/// </summary>
public sealed class ItemValidator
{
    public ValidationResult Validate(Item? item)
    {
        if (item is null)
            return ValidationResult.Failure(new[] { "Item is required." });
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(item.Id))
            errors.Add("Id is required.");
        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}
