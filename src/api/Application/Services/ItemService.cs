using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>
/// Application service for Item use cases. Depends on IRepository for persistence (AC-FOUNDATION-012.6).
/// </summary>
public sealed class ItemService : IItemService
{
    private readonly IRepository<Item> _repository;

    public ItemService(IRepository<Item> repository)
    {
        _repository = repository;
    }

    public Task<Item?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Task.FromResult<Item?>(null);
        return _repository.GetByIdAsync(id, id, cancellationToken);
    }
}
