namespace AIKit.Core.Ingestion;

/// <summary>
/// Represents a delegate for the next step in the ingestion pipeline.
/// </summary>
/// <typeparam name="TContext">The type of the context.</typeparam>
/// <param name="ctx">The context.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task IngestionDelegate<TContext>(TContext ctx);

/// <summary>
/// Defines the contract for middleware in the ingestion pipeline.
/// </summary>
/// <typeparam name="TContext">The type of the context.</typeparam>
public interface IIngestionMiddleware<TContext>
{
    /// <summary>
    /// Invokes the middleware asynchronously.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvokeAsync(TContext ctx, IngestionDelegate<TContext> next);
}