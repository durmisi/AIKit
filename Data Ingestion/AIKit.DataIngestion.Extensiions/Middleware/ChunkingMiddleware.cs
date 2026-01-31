using AIKit.DataIngestion.Services.Chunking;
using AIKit.DataIngestion.Services.Processors;
using Microsoft.Extensions.DataIngestion;

namespace AIKit.DataIngestion.Middleware;

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
        var allChunks = new List<IngestionChunk<string>>();

        foreach (var doc in ctx.Documents)
        {
            var docChunks = await _chunkingStrategy.Chunk(doc);
            var docChunksList = new List<IngestionChunk<string>>(docChunks);

            var chunksAsync = docChunksList.ToAsyncEnumerable();

            foreach (var processor in _chunkProcessors)
            {
                chunksAsync = processor.ProcessAsync(chunksAsync, cancellationToken);
            }

            var processedDocChunks = await chunksAsync.ToListAsync(cancellationToken);
            ctx.DocumentChunks[doc.Identifier] = processedDocChunks;
            allChunks.AddRange(processedDocChunks);
        }

        await next(ctx, cancellationToken);
    }
}