namespace AIKit.Core.Ingestion.Services;

public interface IChunkingStrategy
{
    IReadOnlyList<DocumentChunk> Chunk(IngestionDocument document);
}
