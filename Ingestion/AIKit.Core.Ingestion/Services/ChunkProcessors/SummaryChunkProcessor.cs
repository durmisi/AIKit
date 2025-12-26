using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.ChunkProcessors;

public sealed class SummaryChunkProcessor : IChunkProcessor
{
    private readonly SummaryEnricher _enricher;

    public SummaryChunkProcessor(SummaryEnricher enricher)
    {
        _enricher = enricher;
    }

    public async IAsyncEnumerable<IngestionChunk<string>> ProcessAsync(IAsyncEnumerable<IngestionChunk<string>> chunks, CancellationToken ct)
    {
       await foreach (var chunk in _enricher.ProcessAsync(chunks, ct))
       {
           yield return chunk;
       }
    }
}
