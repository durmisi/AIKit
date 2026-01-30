using Microsoft.Extensions.Logging;

namespace AIKit.Core.Ingestion.Middleware;

/// <summary>
/// Middleware that catches exceptions during pipeline execution and adds error messages to the context.
/// </summary>
/// <typeparam name="T">The type of the context, must inherit from <see cref="IngestionContext"/>.</typeparam>
public class ErrorHandlingMiddleware<T> : IIngestionMiddleware<T> where T : IngestionContext
{
    /// <summary>
    /// Invokes the middleware, wrapping the next delegate in a try-catch block.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(T ctx, IngestionDelegate<T> next, CancellationToken cancellationToken = default)
    {
        var logger = (ctx as DataIngestionContext)?.LoggerFactory?.CreateLogger("ErrorHandlingMiddleware");
        try
        {
            await next(ctx, cancellationToken);
        }
        catch (Exception ex)
        {
            ctx.Errors.Add(ex.Message);
            logger?.LogError(ex, "An error occurred during pipeline execution");
        }
    }
}