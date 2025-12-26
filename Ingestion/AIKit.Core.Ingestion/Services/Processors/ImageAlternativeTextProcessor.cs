using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Processors;

public sealed class ImageAlternativeTextProcessor : IIngestionDocumentProcessor
{
    private readonly IngestionDocumentProcessor _enricher;

    public ImageAlternativeTextProcessor(IngestionDocumentProcessor enricher)
    {
        _enricher = enricher;
    }

    public async Task<IngestionDocument> ProcessAsync(IngestionDocument ingestionDocument, CancellationToken ct)
    {
        var ingestionDocumentProcessed = await _enricher.ProcessAsync(ingestionDocument, ct);
        return ingestionDocumentProcessed;
    }
}