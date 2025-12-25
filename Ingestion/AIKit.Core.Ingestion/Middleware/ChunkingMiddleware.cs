using AIKit.Core.Ingestion.Services.Chunking;
using Microsoft.Extensions.Logging;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class ChunkingMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IChunkingStrategy _chunkingStrategy;

    public ChunkingMiddleware(IChunkingStrategy chunkingStrategy)
    {
        _chunkingStrategy = chunkingStrategy;
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("ChunkingMiddleware");
        logger?.LogInformation("Starting chunking for {DocumentCount} documents", ctx.Documents.Count);

        ctx.Properties["chunks"] = (await Task.WhenAll(ctx.Documents.Select(d => _chunkingStrategy.Chunk(d)))).SelectMany(c => c).ToList();

        logger?.LogInformation("Chunking completed, produced {ChunkCount} chunks", ((List<DocumentChunk>)ctx.Properties["chunks"]).Count);

        await next(ctx);
    }
}
