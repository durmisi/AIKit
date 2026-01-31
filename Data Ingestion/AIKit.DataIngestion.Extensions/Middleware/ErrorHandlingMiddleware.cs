namespace AIKit.DataIngestion.Middleware;

/// <summary>
/// Middleware that catches exceptions during pipeline execution and adds error messages to the context.
/// </summary>
public class ErrorHandlingMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    /// <summary>
    /// Invokes the middleware, wrapping the next delegate in a try-catch block.
    /// </summary>
    /// <param name="ctx">The context.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(DataIngestionContext ctx, IngestionDelegate<DataIngestionContext> next, CancellationToken cancellationToken = default)
    {
        try
        {
            await next(ctx, cancellationToken);
        }
        catch (Exception ex)
        {
            ctx.Errors.Add(ex.Message);
        }
    }
}