using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AIKit.DataIngestion;

/// <summary>
/// Represents a pipeline for executing middleware on a context of type T.
/// </summary>
/// <typeparam name="T">The type of the context.</typeparam>
public class IngestionPipeline<T> where T : IngestionContext
{
    private static readonly ActivitySource ActivitySource = new("AIKit.DataIngestion");
    private static readonly Meter Meter = new("AIKit.DataIngestion");
    private static readonly Histogram<double> ExecutionTimeHistogram = Meter.CreateHistogram<double>("ingestion_pipeline_execution_time", "ms", "Time taken to execute the ingestion pipeline");
    private static readonly Counter<long> MiddlewareCountCounter = Meter.CreateCounter<long>("ingestion_pipeline_middleware_count", description: "Number of middleware components in the pipeline");
    private static readonly Counter<long> ExecutionCounter = Meter.CreateCounter<long>("ingestion_pipeline_executions", description: "Number of pipeline executions");
    private static readonly Counter<long> ErrorCounter = Meter.CreateCounter<long>("ingestion_pipeline_errors", description: "Number of pipeline execution errors");

    private readonly List<Func<IngestionDelegate<T>, IngestionDelegate<T>>> _components;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly RetryPolicy? _retryPolicy;
    private readonly TelemetryOptions _telemetryOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="IngestionPipeline{T}"/> class.
    /// </summary>
    /// <param name="components">The list of middleware components.</param>
    /// <param name="loggerFactory">The logger factory for logging.</param>
    /// <param name="retryPolicy">The retry policy.</param>
    /// <param name="telemetryOptions">The telemetry options.</param>
    public IngestionPipeline(List<Func<IngestionDelegate<T>, IngestionDelegate<T>>> components, ILoggerFactory? loggerFactory = null, RetryPolicy? retryPolicy = null, TelemetryOptions? telemetryOptions = null)
    {
        _components = components;
        _loggerFactory = loggerFactory;
        _retryPolicy = retryPolicy;
        _telemetryOptions = telemetryOptions ?? new TelemetryOptions();
    }

    /// <summary>
    /// Executes the pipeline asynchronously on the specified context.
    /// </summary>
    /// <param name="ctx">The context to process.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task ExecuteAsync(T ctx, CancellationToken cancellationToken = default)
    {
        var logger = _loggerFactory?.CreateLogger("IngestionPipeline");
        logger?.LogInformation("Starting pipeline execution");

        using var activity = _telemetryOptions.Enabled ? ActivitySource.StartActivity("IngestionPipeline.Execute") : null;
        var stopwatch = Stopwatch.StartNew();

        IngestionDelegate<T> app = (c, ct) => Task.CompletedTask;
        for (int i = _components.Count - 1; i >= 0; i--)
        {
            app = _components[i](app);
        }

        if (_telemetryOptions.Enabled)
        {
            MiddlewareCountCounter.Add(_components.Count);
        }

        var task = ExecuteWithRetryAsync(() => app(ctx, cancellationToken), cancellationToken);

        task.ContinueWith(t =>
        {
            stopwatch.Stop();
            var executionTime = stopwatch.Elapsed.TotalMilliseconds;

            if (_telemetryOptions.Enabled)
            {
                ExecutionTimeHistogram.Record(executionTime);
                ExecutionCounter.Add(1);

                activity?.SetTag("execution_time_ms", executionTime);
                activity?.SetTag("middleware_count", _components.Count);
            }

            if (t.IsCompletedSuccessfully)
            {
                if (_telemetryOptions.Enabled)
                {
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                logger?.LogInformation("Pipeline execution completed successfully");
            }
            else if (t.IsFaulted)
            {
                if (_telemetryOptions.Enabled)
                {
                    ErrorCounter.Add(1);
                    activity?.SetStatus(ActivityStatusCode.Error, t.Exception?.Message);
                }
                logger?.LogError(t.Exception, "Pipeline execution failed");
                ctx.Errors.Add(t.Exception?.Message ?? "Unknown error");
            }
            else if (t.IsCanceled)
            {
                if (_telemetryOptions.Enabled)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "Pipeline execution was canceled");
                }
                logger?.LogWarning("Pipeline execution was canceled");
            }
        }, cancellationToken);

        return task;
    }

    private async Task ExecuteWithRetryAsync(Func<Task> action, CancellationToken ct)
    {
        if (_retryPolicy == null)
        {
            await action();
            return;
        }

        for (int i = 0; i <= _retryPolicy.MaxRetries; i++)
        {
            try
            {
                await action();
                return;
            }
            catch (Exception) when (i < _retryPolicy.MaxRetries)
            {
                await Task.Delay(_retryPolicy.Delay, ct);
            }
        }
    }
}