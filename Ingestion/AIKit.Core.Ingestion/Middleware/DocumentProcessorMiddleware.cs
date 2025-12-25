using AIKit.Core.Ingestion.Services.Processors;
using Microsoft.Extensions.Logging;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class DocumentProcessorMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IEnumerable<IDocumentProcessor> _processors;

    public DocumentProcessorMiddleware(IEnumerable<IDocumentProcessor> processors)
    {
        _processors = processors;
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("DocumentProcessorMiddleware");
        logger?.LogInformation("Processing {DocumentCount} documents with {ProcessorCount} processors", ctx.Documents.Count, _processors.Count());

        foreach (var doc in ctx.Documents)
        {
            foreach (var processor in _processors)
            {
                await processor.ProcessAsync(doc, CancellationToken.None);
            }
        }

        logger?.LogInformation("Document processing completed");

        await next(ctx);
    }
}
