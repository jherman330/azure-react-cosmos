using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Validation;

/// <summary>
/// Validates Item entity for application-level rules. Used for example unit testing (AC-FOUNDATION-012.7).
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
