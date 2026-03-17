using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Services;

/// <summary>
/// Application service for Item use cases. Used for example unit testing with mocked IRepository (AC-FOUNDATION-012.6).
/// </summary>
public interface IItemService
{
    Task<Item?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
}
