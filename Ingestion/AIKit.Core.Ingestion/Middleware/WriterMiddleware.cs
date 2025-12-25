using AIKit.Core.Ingestion.Services.Writers;

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
        var chunks = (IReadOnlyList<DocumentChunk>)ctx.Properties["chunks"];
        await _writer.WriteAsync(chunks, CancellationToken.None);
        await next(ctx);
    }
}
