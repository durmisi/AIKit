using AIKit.Core.Ingestion.Services.Chunking;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class ChunkingMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IChunkingStrategy _chunkingStrategy;

    public ChunkingMiddleware(IChunkingStrategy chunkingStrategy)
    {
        _chunkingStrategy = chunkingStrategy;
    }

    public async Task InvokeAsync(
        DataIngestionContext context,
        IngestionDelegate<DataIngestionContext> next)
    {
        var allChunks = new List<DocumentChunk>();

        foreach (var document in context.Documents)
        {
            var chunks = await _chunkingStrategy.Chunk(document);
            allChunks.AddRange(chunks);
        }

        context.Properties["chunks"] = allChunks;

        await next(context);
    }
}
