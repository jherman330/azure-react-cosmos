namespace Todo.Api.Domain.Repositories;

/// <summary>
/// Simple query descriptor for repository query operations.
/// Infrastructure translates this to the underlying store. Query text and parameter names
/// are defined by the concrete repository (e.g. Cosmos SQL with @param names); Domain has no store reference.
/// </summary>
/// <param name="QueryText">Query text (e.g. SQL for Cosmos DB).</param>
/// <param name="Parameters">Optional named parameters.</param>
public sealed record QuerySpec(
    string QueryText,
    IReadOnlyDictionary<string, object>? Parameters = null);
