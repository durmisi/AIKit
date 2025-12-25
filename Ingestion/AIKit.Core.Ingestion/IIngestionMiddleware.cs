namespace AIKit.Core.Ingestion;

public delegate Task IngestionDelegate<TContext>(TContext ctx);
public interface IIngestionMiddleware<TContext>
{
    Task InvokeAsync(TContext ctx, IngestionDelegate<TContext> next);
}