using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Processors;

public sealed class ImageAlternativeTextProcessor : IDocumentProcessor
{
    private readonly IngestionDocumentProcessor _enricher;

    public ImageAlternativeTextProcessor(IngestionDocumentProcessor enricher)
    {
        _enricher = enricher;
    }

    public async Task ProcessAsync(IngestionDocument document, CancellationToken ct)
    {
        var dataDoc = new Microsoft.Extensions.DataIngestion.IngestionDocument(document.Content);
        await _enricher.ProcessAsync(dataDoc, ct);
    }
}