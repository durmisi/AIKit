using AIKit.Core.Ingestion.Services.Readers;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class ReaderMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IIngestionDocumentProvider _reader;

    public ReaderMiddleware(IIngestionDocumentProvider reader)
    {
        _reader = reader;
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        await foreach (var doc in _reader.ReadAsync())
        {
            ctx.Documents.Add(doc);
        }

        await next(ctx);
    }
}
