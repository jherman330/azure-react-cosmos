namespace Todo.Api.Application.Validation;

/// <summary>
/// Result of validating an entity or input. Used by validators for example unit testing (AC-FOUNDATION-012.7).
/// </summary>
public sealed class ValidationResult
{
    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }

    private ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success() => new(true, Array.Empty<string>());
    public static ValidationResult Failure(IReadOnlyList<string> errors) => new(false, errors ?? Array.Empty<string>());
}
