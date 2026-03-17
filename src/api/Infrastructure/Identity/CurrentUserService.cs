using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Todo.Api.Application.Services;

namespace Todo.Api.Infrastructure.Identity;

/// <summary>
/// Resolves the current user identifier from the HttpContext claims principal (AC-FOUNDATION-003.7).
/// Audit identity standard: primary is "oid" (Object ID), fallback is "sub" (subject). All audit fields (CreatedBy, UpdatedBy) use this value.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    /// <remarks>Uses oid (Object ID) as primary for stable audit identity; falls back to sub if oid is not present.</remarks>
    public string? UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("oid")
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
}
