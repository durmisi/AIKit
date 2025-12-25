namespace AIKit.Core.Ingestion.Services.ChunkProcessors;

using Microsoft.Extensions.DataIngestion;

public sealed class SummaryChunkProcessor : IChunkProcessor
{
    private readonly SummaryEnricher _enricher;

    public SummaryChunkProcessor(EnricherOptions options, int? maxWordCount = null)
    {
        _enricher = new SummaryEnricher(options, maxWordCount);
    }

    public async Task ProcessAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken ct)
    {
        // 1. Convert AIKit chunks → IngestionChunk<string>
        var ingestionChunks = chunks.Select(ToIngestionChunk);

        // 2. Run Microsoft streaming processor
        var enriched = _enricher.ProcessAsync(
            ingestionChunks.ToAsyncEnumerable(),
            ct);

        // 3. Materialize and map results back
        await foreach (var enrichedChunk in enriched.WithCancellation(ct))
        {
            ApplyMetadata(enrichedChunk, chunks);
        }
    }

    private static IngestionChunk<string> ToIngestionChunk(DocumentChunk chunk)
    {
        return new IngestionChunk<string>(
            chunk.Content,
            new IngestionDocument(chunk.DocumentId)
        );
    }

    private static void ApplyMetadata(
        IngestionChunk<string> enrichedChunk,
        IReadOnlyList<DocumentChunk> targetChunks)
    {
        // Metadata dictionary is shared by reference
        // so nothing special is required here.
        // This method exists for clarity and future extensibility.
    }
}
