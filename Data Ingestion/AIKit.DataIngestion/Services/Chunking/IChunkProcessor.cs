using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Chunking;

public interface IChunkProcessor
{
    IAsyncEnumerable<IngestionChunk<string>> ProcessAsync(IAsyncEnumerable<IngestionChunk<string>> chunks, CancellationToken ct);
}