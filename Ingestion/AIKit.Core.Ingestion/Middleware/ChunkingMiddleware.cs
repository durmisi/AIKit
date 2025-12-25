using AIKit.Core.Ingestion.Services;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class ChunkingMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IChunkingStrategy _strategy;

    public ChunkingMiddleware(IChunkingStrategy strategy)
    {
        _strategy = strategy;
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        ctx.Properties["chunks"] = ctx.Documents
            .SelectMany(d => _strategy.Chunk(d))
            .ToList();

        await next(ctx);
    }
}
