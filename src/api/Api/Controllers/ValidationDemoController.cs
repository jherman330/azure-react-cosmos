using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Application.Transport;
using Todo.Api.Infrastructure;

namespace Todo.Api.Api.Controllers;

/// <summary>
/// Demonstrates automatic FluentValidation before action execution. Safe for integration tests; protect or remove for production.
/// </summary>
[ApiController]
[Route("api/v1/sandbox")]
public sealed class ValidationDemoController : ControllerBase
{
    /// <summary>Returns the message when validation passes.</summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiValidationErrorEnvelope), StatusCodes.Status400BadRequest)]
    public IActionResult Validate([FromBody] ValidationDemoRequest request)
    {
        return Ok(new { message = request.Message });
    }
}
