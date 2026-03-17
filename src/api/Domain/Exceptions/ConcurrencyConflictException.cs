namespace Todo.Api.Domain.Exceptions;

/// <summary>
/// Thrown when an update or delete fails due to ETag mismatch (optimistic concurrency conflict).
/// Cosmos DB returns 412 Precondition Failed in this case.
/// Callers can catch this to refresh and retry or report conflict to the user.
/// </summary>
public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}
