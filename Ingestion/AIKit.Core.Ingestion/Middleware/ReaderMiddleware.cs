using AIKit.Core.Ingestion.Services.Processors;
using Microsoft.Extensions.Logging;
using AIKit.Core.Ingestion.Services.Providers;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class ReaderMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IIngestionDocumentProvider _reader;
    private readonly IEnumerable<IIngestionDocumentProcessor> _processors;

    public ReaderMiddleware(IIngestionDocumentProvider reader, IEnumerable<IIngestionDocumentProcessor>? processors = null)
    {
        _reader = reader;
        _processors = processors ?? Array.Empty<IIngestionDocumentProcessor>();
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("ReaderMiddleware");
        logger?.LogInformation("Starting document reading");

        await foreach (var doc in _reader.ReadAsync())
        {
            // Apply processors to the document
            foreach (var processor in _processors)
            {
                await processor.ProcessAsync(doc, CancellationToken.None);
            }

            ctx.Documents.Add(doc);
        }

        logger?.LogInformation("Reading completed, loaded {DocumentCount} documents", ctx.Documents.Count);

        await next(ctx);
    }
}
