using Todo.Api.Infrastructure.Middleware;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// Registers global exception handling middleware (AC-FOUNDATION-007).
/// </summary>
public static class ExceptionHandlingServiceCollectionExtensions
{
    /// <summary>
    /// Adds global exception handling middleware that catches unhandled exceptions and returns
    /// standardized error envelope (traceId, errorCode, message). Register early so it wraps the pipeline.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
