using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

/// <summary>
/// Example transport validator (AC-FOUNDATION-008). Copy this pattern for <c>CreateXRequestValidator</c>, etc.
/// </summary>
public sealed class ValidationDemoRequestValidator : TransportValidatorBase<ValidationDemoRequest>
{
    public ValidationDemoRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(500)
            .WithMessage("Message must not exceed 500 characters.");
    }
}
