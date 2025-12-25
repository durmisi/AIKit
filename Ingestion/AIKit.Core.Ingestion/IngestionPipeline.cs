namespace AIKit.Core.Ingestion;

/// <summary>
/// Represents a pipeline for executing middleware on a context of type T.
/// </summary>
/// <typeparam name="T">The type of the context.</typeparam>
public class IngestionPipeline<T>
{
    private readonly IList<Func<IngestionDelegate<T>, IngestionDelegate<T>>> _components;

    /// <summary>
    /// Initializes a new instance of the <see cref="IngestionPipeline{T}"/> class.
    /// </summary>
    /// <param name="c">The list of middleware components.</param>
    public IngestionPipeline(IList<Func<IngestionDelegate<T>, IngestionDelegate<T>>> c)
    {
        _components = c;
    }

    /// <summary>
    /// Executes the pipeline asynchronously on the specified context.
    /// </summary>
    /// <param name="ctx">The context to process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteAsync(T ctx)
    {
        IngestionDelegate<T> app = _ => Task.CompletedTask;
        foreach (var c in _components.Reverse()) app = c(app);
        return app(ctx);
    }
}