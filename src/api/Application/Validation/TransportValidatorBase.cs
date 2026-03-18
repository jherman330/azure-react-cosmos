using FluentValidation;

namespace Todo.Api.Application.Validation;

/// <summary>
/// Base for <b>inbound API request body</b> validators only. Subclass per transport type in <c>Application.Transport</c>.
/// Shared default: stop after first failure per rule chain. Otherwise use normal FluentValidation APIs.
/// </summary>
public abstract class TransportValidatorBase<T> : AbstractValidator<T>
{
    protected TransportValidatorBase()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
    }
}
