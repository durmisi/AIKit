using Microsoft.Extensions.DataIngestion;

namespace AIKit.DataIngestion.Services.Processors;

public interface IChunkProcessor
{
    IAsyncEnumerable<IngestionChunk<string>> ProcessAsync(IAsyncEnumerable<IngestionChunk<string>> chunks, CancellationToken ct);
}