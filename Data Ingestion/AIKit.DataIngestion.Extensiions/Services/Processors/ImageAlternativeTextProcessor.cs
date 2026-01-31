using Microsoft.Extensions.DataIngestion;

namespace AIKit.DataIngestion.Services.Processors;

public sealed class ImageAlternativeTextProcessor : IIngestionDocumentProcessor
{
    private readonly IngestionDocumentProcessor _enricher;

    public ImageAlternativeTextProcessor(EnricherOptions options)
    {
        _enricher = new ImageAlternativeTextEnricher(options);
    }

    public async Task<IngestionDocument> ProcessAsync(IngestionDocument ingestionDocument, CancellationToken ct)
    {
        return await _enricher.ProcessAsync(ingestionDocument, ct);
    }
}