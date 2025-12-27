using AIKit.Core.Ingestion.Services.Processors;
using Microsoft.Extensions.Logging;
using AIKit.Core.Ingestion.Services.Providers;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class ReaderMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IIngestionDocumentProvider _documentProvider;
    private readonly IEnumerable<IIngestionDocumentProcessor> _processors;

    public ReaderMiddleware(
        IIngestionDocumentProvider documentProvider,
        IEnumerable<IIngestionDocumentProcessor>? processors = null)
    {
        _documentProvider = documentProvider;
        _processors = processors ?? [];
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("ReaderMiddleware");
        logger?.LogInformation("Starting document reading");

        await foreach (var doc in _documentProvider.ReadAsync())
        {
            // Apply processors to the document
            var doc1 = doc;
            foreach (var processor in _processors)
            {
                var processedDocument = await processor.ProcessAsync(doc, CancellationToken.None);
                doc1 = processedDocument ?? doc1;
            }

            ctx.Documents.Add(doc1);
        }

        logger?.LogInformation("Reading completed, loaded {DocumentCount} documents", ctx.Documents.Count);

        await next(ctx);
    }
}
