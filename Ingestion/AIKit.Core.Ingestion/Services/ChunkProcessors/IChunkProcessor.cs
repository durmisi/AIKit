using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.ChunkProcessors;

public interface IChunkProcessor
{
    IAsyncEnumerable<IngestionChunk<string>> ProcessAsync(IAsyncEnumerable<IngestionChunk<string>> chunks, CancellationToken ct);
}