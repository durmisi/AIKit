using Microsoft.Extensions.Logging;

namespace AIKit.DataIngestion;

/// <summary>
/// Builder for constructing an <see cref="IngestionPipeline{T}"/> with a fluent API.
/// </summary>
/// <typeparam name="T">The type of the context.</typeparam>
public class IngestionPipelineBuilder<T> where T : IngestionContext
{
    private readonly List<Func<IngestionDelegate<T>, IngestionDelegate<T>>> _components = new();
    private ILoggerFactory? _loggerFactory;
    private RetryPolicy? _retryPolicy;
    private TelemetryOptions _telemetryOptions = new();

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
    public IngestionPipelineBuilder<T> UseMiddleware<TMiddleware>() where TMiddleware : class, new()
    {
        return Use(next => async (ctx, ct) =>
        {
            var middleware = new TMiddleware();
            var method = typeof(TMiddleware).GetMethod("InvokeAsync", new Type[] { typeof(T), typeof(IngestionDelegate<T>), typeof(CancellationToken) });
            if (method == null)
            {
                throw new InvalidOperationException($"Middleware {typeof(TMiddleware)} must have a method InvokeAsync({typeof(T).Name} context, {typeof(IngestionDelegate<T>).Name} next, CancellationToken cancellationToken)");
            }
            await (Task)method.Invoke(middleware, new object[] { ctx, next, ct });
        });
    }

    /// <summary>
    /// Adds conditional middleware execution.
    /// </summary>
    /// <param name="predicate">The predicate to check.</param>
    /// <param name="configure">The configuration action for the conditional pipeline.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IngestionPipelineBuilder<T> UseWhen(Func<T, bool> predicate, Action<IngestionPipelineBuilder<T>> configure)
    {
        var whenBuilder = new IngestionPipelineBuilder<T>();
        configure(whenBuilder);
        var whenPipeline = whenBuilder.Build();
        _components.Add(next => async (ctx, ct) =>
        {
            if (predicate(ctx))
            {
                await whenPipeline.ExecuteAsync(ctx, ct);
            }
            await next(ctx, ct);
        });
        return this;
    }

    /// <summary>
    /// Sets the logger factory for the pipeline.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IngestionPipelineBuilder<T> WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    /// <summary>
    /// Sets the retry policy for the pipeline.
    /// </summary>
    /// <param name="policy">The retry policy.</param>
    /// <returns>The builder instance for chaining.</returns>
    public IngestionPipelineBuilder<T> WithRetryPolicy(RetryPolicy policy)
    {
        _retryPolicy = policy;
        return this;
    }

    /// <summary>
    /// Enables telemetry for the pipeline.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public IngestionPipelineBuilder<T> WithTelemetry()
    {
        _telemetryOptions.Enabled = true;
        return this;
    }

    /// <summary>
    /// Builds the pipeline with the added middleware.
    /// </summary>
    /// <returns>The constructed pipeline.</returns>
    public IngestionPipeline<T> Build()
    {
        Validate();
        return new IngestionPipeline<T>(_components, _loggerFactory, _retryPolicy, _telemetryOptions);
    }

    private void Validate()
    {
        if (_components.Count == 0)
        {
            throw new InvalidOperationException("No middleware added to the pipeline.");
        }
        // Additional validations can be added here
    }
}