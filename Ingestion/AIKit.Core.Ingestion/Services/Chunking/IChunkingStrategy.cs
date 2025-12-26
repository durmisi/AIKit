using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Chunking;

public interface IChunkingStrategy
{
    Task<IReadOnlyList<IngestionChunk<string>>> Chunk(IngestionDocument document);
}
    