namespace AIKit.Core.Ingestion;

public class IngestionPipeline<T>
{
    private readonly IList<Func<IngestionDelegate<T>, IngestionDelegate<T>>> _components;
    public IngestionPipeline(IList<Func<IngestionDelegate<T>, IngestionDelegate<T>>> c)
    {
        _components = c;
    }
    public Task ExecuteAsync(T ctx)
    {
        IngestionDelegate<T> app = _ => Task.CompletedTask;
        foreach (var c in _components.Reverse()) app = c(app);
        return app(ctx);
    }
}