using AIKit.Core.Ingestion.Services.Writers;
using Microsoft.Extensions.DataIngestion;
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
        IngestionDelegate<DataIngestionContext> next,
        CancellationToken cancellationToken = default)
    {
        var logger = ctx.LoggerFactory?.CreateLogger("WriterMiddleware");

        foreach (var doc in ctx.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunks = ctx.DocumentChunks.TryGetValue(doc.Identifier, out var docChunks)
                ? docChunks
                : Array.Empty<IngestionChunk<string>>().ToList();

            await _writer.WriteAsync(doc, chunks, cancellationToken);
        }

        logger?.LogInformation("Writing completed");

        await next(ctx, cancellationToken);
    }
}
