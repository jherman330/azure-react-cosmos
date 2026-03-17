namespace Todo.Api.Application.Services;

/// <summary>
/// Provides the current authenticated user identifier from the request (AC-FOUNDATION-003.7).
/// Used to populate audit fields (CreatedBy, UpdatedBy). Standard: oid (Object ID) primary, fallback sub.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's audit identifier: oid (Object ID) if present, otherwise sub (subject), or null if unauthenticated.
    /// </summary>
    string? UserId { get; }
}
