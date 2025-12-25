namespace AIKit.Core.Ingestion.Services.Chunking;

public interface IChunkingStrategy
{
    Task<IReadOnlyList<DocumentChunk>> Chunk(IngestionDocument document);
}
