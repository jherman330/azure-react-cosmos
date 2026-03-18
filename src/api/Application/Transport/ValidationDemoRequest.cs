namespace Todo.Api.Application.Transport;

/// <summary>
/// Inbound JSON body for the sandbox validate endpoint—not a domain entity.
/// Real endpoints: add one transport type per action here (or equivalent) and a matching <c>*Validator</c>.
/// </summary>
public sealed class ValidationDemoRequest
{
    /// <summary>Required non-empty message (max 500 characters).</summary>
    public string Message { get; set; } = string.Empty;
}
