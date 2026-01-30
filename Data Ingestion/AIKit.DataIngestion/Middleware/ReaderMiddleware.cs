using AIKit.DataIngestion.Services.Processors;
using AIKit.DataIngestion.Services.Providers;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;

namespace AIKit.DataIngestion.Middleware;

public sealed class ReaderMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IIngestionFileProvider _fileProvider;
    private readonly Dictionary<string, IngestionDocumentReader> _readers;
    private readonly Dictionary<string, IEnumerable<IIngestionDocumentProcessor>> _processorsPerExtension;

    public ReaderMiddleware(
        IIngestionFileProvider fileProvider,
        Dictionary<string, IngestionDocumentReader> readers,
        Dictionary<string, IEnumerable<IIngestionDocumentProcessor>>? processorsPerExtension = null)
    {
        _fileProvider = fileProvider;
        _readers = readers ?? throw new ArgumentNullException(nameof(readers));
        _processorsPerExtension = processorsPerExtension ?? new Dictionary<string, IEnumerable<IIngestionDocumentProcessor>>();
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next,
        CancellationToken cancellationToken = default)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("ReaderMiddleware");
        logger?.LogInformation("Starting document reading");

        await foreach (var file in _fileProvider.ReadAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var extension = file.Extension.ToLowerInvariant();
            if (_readers.TryGetValue(extension, out var reader))
            {
                // Use the stream overload of the reader
                await using (var stream = await file.OpenReadAsync(cancellationToken))
                {
                    var processedDoc = await reader.ReadAsync(stream, file.Name, file.Extension, cancellationToken);

                    // Apply processors for this extension
                    if (_processorsPerExtension.TryGetValue(extension, out var processors))
                    {
                        foreach (var processor in processors)
                        {
                            processedDoc = await processor.ProcessAsync(processedDoc, cancellationToken);
                        }
                    }

                    ctx.Documents.Add(processedDoc);
                }
            }
            // Skip files with unsupported extensions
        }

        logger?.LogInformation("Reading completed, loaded {DocumentCount} documents", ctx.Documents.Count);

        await next(ctx, cancellationToken);
    }
}