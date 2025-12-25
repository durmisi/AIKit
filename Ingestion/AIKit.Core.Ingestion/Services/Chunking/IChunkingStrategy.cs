namespace AIKit.Core.Ingestion.Services.Chunking;

public interface IChunkingStrategy
{
    IReadOnlyList<DocumentChunk> Chunk(IngestionDocument document);
}
