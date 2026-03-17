namespace Todo.Api.Domain.Entities;

/// <summary>
/// Entity that supports audit fields. Repository implementations
/// populate these automatically on create and update (AC-FOUNDATION-002.5).
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset? CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}
