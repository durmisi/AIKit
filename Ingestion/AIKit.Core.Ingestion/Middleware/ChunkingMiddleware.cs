using AIKit.Core.Ingestion.Services.Chunking;
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
        IngestionDelegate<DataIngestionContext> next,
        CancellationToken cancellationToken = default)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("ChunkingMiddleware");
        logger?.LogInformation("Starting chunking for {DocumentCount} documents", ctx.Documents.Count);

        var allChunks = new List<IngestionChunk<string>>();

        foreach (var doc in ctx.Documents)
        {
            var docChunks = await _chunkingStrategy.Chunk(doc);
            var docChunksList = new List<IngestionChunk<string>>(docChunks);

            var chunksAsync = docChunksList.ToAsyncEnumerable();

            foreach (var processor in _chunkProcessors)
            {
                logger?.LogInformation("Applying chunk processor: {ProcessorName}", processor.GetType().Name);
                chunksAsync = processor.ProcessAsync(chunksAsync, cancellationToken);
            }

            var processedDocChunks = await chunksAsync.ToListAsync(cancellationToken);
            ctx.DocumentChunks[doc.Identifier] = processedDocChunks;
            allChunks.AddRange(processedDocChunks);
        }

        logger?.LogInformation("Chunking completed, produced {ChunkCount} chunks", allChunks.Count);

        await next(ctx, cancellationToken);
    }
}
