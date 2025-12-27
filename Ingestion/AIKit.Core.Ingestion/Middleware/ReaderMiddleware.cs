using AIKit.Core.Ingestion.Services.Processors;
using AIKit.Core.Ingestion.Services.Providers;
using Microsoft.Extensions.Logging;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class ReaderMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IIngestionDocumentProvider _documentProvider;
    private readonly Dictionary<string, IEnumerable<IIngestionDocumentProcessor>> _processorsPerExtension;

    public ReaderMiddleware(
        IIngestionDocumentProvider documentProvider,
        Dictionary<string, IEnumerable<IIngestionDocumentProcessor>>? processorsPerExtension = null)
    {
        _documentProvider = documentProvider;
        _processorsPerExtension = processorsPerExtension ?? new Dictionary<string, IEnumerable<IIngestionDocumentProcessor>>();
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("ReaderMiddleware");
        logger?.LogInformation("Starting document reading");

        await foreach (var doc in _documentProvider.ReadAsync())
        {
            var processedDoc = doc;

            // Determine the file extension from the document's URI
            string extension = string.Empty;
            var dynamicDoc = (dynamic)processedDoc;
            if (dynamicDoc.Uri is not null)
            {
                extension = Path.GetExtension(dynamicDoc.Uri.LocalPath).ToLowerInvariant();
            }

            // Apply processors for this extension
            if (_processorsPerExtension.TryGetValue(extension, out var processors))
            {
                foreach (var processor in processors)
                {
                    processedDoc = await processor.ProcessAsync(processedDoc, CancellationToken.None);
                }
            }

            ctx.Documents.Add(processedDoc);
        }

        logger?.LogInformation("Reading completed, loaded {DocumentCount} documents", ctx.Documents.Count);

        await next(ctx);
    }
}
