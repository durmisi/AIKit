using AIKit.Core.Ingestion.Services.Chunking;
using AIKit.Core.Ingestion.Services.ChunkProcessors;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class ChunkingMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IChunkingStrategy _chunkingStrategy;
    private readonly IEnumerable<IChunkProcessor> _chunkProcessors;

    public ChunkingMiddleware(IChunkingStrategy chunkingStrategy, IEnumerable<IChunkProcessor> chunkProcessors)
    {
        _chunkingStrategy = chunkingStrategy;
        _chunkProcessors = chunkProcessors;
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("ChunkingMiddleware");
        logger?.LogInformation("Starting chunking for {DocumentCount} documents", ctx.Documents.Count);

        var chunks = (await Task.WhenAll(ctx.Documents.Select(d => _chunkingStrategy.Chunk(d)))).SelectMany(c => c);

        foreach (var processor in _chunkProcessors)
        {
            logger?.LogInformation("Applying chunk processor: {ProcessorName}", processor.GetType().Name);
            await foreach (var chunk in processor.ProcessAsync(chunks.ToAsyncEnumerable(), CancellationToken.None))
            {
                // Process each chunk as needed
            }
        }

        ctx.Properties["chunks"] = chunks;

        logger?.LogInformation("Chunking completed, produced {ChunkCount} chunks", ((List<IngestionChunker<string>>)ctx.Properties["chunks"]).Count);

        await next(ctx);
    }
}
