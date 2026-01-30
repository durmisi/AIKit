using Microsoft.Extensions.DataIngestion;

namespace AIKit.DataIngestion.Services.Chunking;

public interface IChunkingStrategy
{
    Task<IReadOnlyList<IngestionChunk<string>>> Chunk(IngestionDocument document);
}