namespace AIKit.Core.Middleware;

public class ErrorHandlingMiddleware<T> : IIngestionMiddleware<T> where T : IngestionContext
{
    public async Task InvokeAsync(T ctx, IngestionDelegate<T> next)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            ctx.Errors.Add(ex.Message);
        }
    }
}