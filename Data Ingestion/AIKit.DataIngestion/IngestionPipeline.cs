using AIKit.DataIngestion.Middleware;
using Microsoft.Extensions.Logging;

namespace AIKit.DataIngestion;

/// <summary>
/// Represents a pipeline for executing middleware on a context of type T.
/// </summary>
/// <typeparam name="T">The type of the context.</typeparam>
public class IngestionPipeline<T>
{
    private readonly IList<Func<IngestionDelegate<T>, IngestionDelegate<T>>> _components;
    private readonly ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="IngestionPipeline{T}"/> class.
    /// </summary>
    /// <param name="c">The list of middleware components.</param>
    /// <param name="loggerFactory">The logger factory for logging.</param>
    public IngestionPipeline(IList<Func<IngestionDelegate<T>, IngestionDelegate<T>>> c, ILoggerFactory? loggerFactory = null)
    {
        _components = c;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Executes the pipeline asynchronously on the specified context.
    /// </summary>
    /// <param name="ctx">The context to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteAsync(T ctx, CancellationToken cancellationToken = default)
    {
        if (ctx is DataIngestionContext dataCtx)
        {
            dataCtx.LoggerFactory = _loggerFactory;
        }

        var logger = _loggerFactory?.CreateLogger("IngestionPipeline");
        logger?.LogInformation("Starting pipeline execution");

        IngestionDelegate<T> app = (c, ct) => Task.CompletedTask;
        foreach (var c in _components.Reverse()) app = c(app);

        var task = app(ctx, cancellationToken);

        task.ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                logger?.LogInformation("Pipeline execution completed successfully");
            }
            else if (t.IsFaulted)
            {
                logger?.LogError(t.Exception, "Pipeline execution failed");
            }
            else if (t.IsCanceled)
            {
                logger?.LogWarning("Pipeline execution was canceled");
            }
        }, cancellationToken);

        return task;
    }
}