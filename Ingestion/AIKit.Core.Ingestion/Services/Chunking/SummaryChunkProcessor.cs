using Microsoft.Extensions.DataIngestion;
using System.Runtime.CompilerServices;

namespace AIKit.Core.Ingestion.Services.Chunking;

public sealed class SummaryChunkProcessor : IChunkProcessor
{
    private readonly SummaryEnricher _enricher;

    public SummaryChunkProcessor(SummaryEnricher enricher)
    {
        _enricher = enricher;
    }

    public async IAsyncEnumerable<IngestionChunk<string>> ProcessAsync(
        IAsyncEnumerable<IngestionChunk<string>> chunks,
         [EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var chunk in _enricher.ProcessAsync(chunks, ct))
        {
            yield return chunk;
        }
    }
}
