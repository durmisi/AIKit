using AIKit.Core.Ingestion.Services.Writers;
using Microsoft.Extensions.Logging;

namespace AIKit.Core.Ingestion.Middleware;


public sealed class WriterMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IDocumentWriter _writer;

    public WriterMiddleware(IDocumentWriter writer)
    {
        _writer = writer;
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("WriterMiddleware");
        var chunks = (IReadOnlyList<DocumentChunk>)ctx.Properties["chunks"];
        logger?.LogInformation("Writing {ChunkCount} chunks", chunks.Count);

        await _writer.WriteAsync(chunks, CancellationToken.None);

        logger?.LogInformation("Writing completed");

        await next(ctx);
    }
}
