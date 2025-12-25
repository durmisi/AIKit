namespace AIKit.Core.Ingestion.Services.ChunkProcessors;

public interface IChunkProcessor
{
    Task ProcessAsync(IReadOnlyList<DocumentChunk> chunks, CancellationToken ct);
}