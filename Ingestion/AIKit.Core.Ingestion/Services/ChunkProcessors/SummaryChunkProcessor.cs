using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.ChunkProcessors;

public sealed class SummaryChunkProcessor : IChunkProcessor
{
    private readonly SummaryEnricher _enricher;

    public SummaryChunkProcessor(SummaryEnricher enricher)
    {
        _enricher = enricher;
    }

    public async Task ProcessAsync(IReadOnlyList<DocumentChunk> chunks, CancellationToken ct)
    {
        var ingestionChunks = chunks
            .Select(x => new IngestionChunk<string>(x.Content, new Microsoft.Extensions.DataIngestion.IngestionDocument(x.DocumentId)))
            .ToAsyncEnumerable();

        _enricher.ProcessAsync(ingestionChunks, ct);
    }
}
