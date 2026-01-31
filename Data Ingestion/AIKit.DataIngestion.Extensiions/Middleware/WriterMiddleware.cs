using AIKit.DataIngestion.Services.Writers;
using Microsoft.Extensions.DataIngestion;

namespace AIKit.DataIngestion.Middleware;

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
        foreach (var doc in ctx.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var chunks = ctx.DocumentChunks.TryGetValue(doc.Identifier, out var docChunks)
                ? docChunks
                : Array.Empty<IngestionChunk<string>>().ToList();

            await _writer.WriteAsync(doc, chunks, cancellationToken);
        }

        await next(ctx, cancellationToken);
    }
}