using AIKit.Core.Ingestion.Middleware;

namespace AIKit.Core.Ingestion;

/// <summary>
/// Builder for constructing an <see cref="IngestionPipeline{T}"/> with a fluent API.
/// </summary>
/// <typeparam name="T">The type of the context.</typeparam>
public class IngestionPipelineBuilder<T>
{
    private readonly List<Func<IngestionDelegate<T>, IngestionDelegate<T>>> _components = new();

    /// <summary>
    /// Adds a middleware function to the pipeline.
    /// </summary>
    /// <param name="middleware">The middleware function.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IngestionPipelineBuilder<T> Use(Func<IngestionDelegate<T>, IngestionDelegate<T>> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    /// <summary>
    /// Adds a middleware class to the pipeline.
    /// </summary>
    /// <typeparam name="TMiddleware">The type of the middleware class.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
    public IngestionPipelineBuilder<T> UseMiddleware<TMiddleware>() where TMiddleware : IIngestionMiddleware<T>, new()
    {
        return Use(next => async ctx => await new TMiddleware().InvokeAsync(ctx, next));
    }

    /// <summary>
    /// Builds the pipeline with the added middleware.
    /// </summary>
    /// <returns>The constructed pipeline.</returns>
    public IngestionPipeline<T> Build()
    {
        return new IngestionPipeline<T>(_components);
    }
}