using AIKit.Core.Ingestion.Services.Readers;
using Microsoft.Extensions.Logging;

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
        var logger = ctx.LoggerFactory?.CreateLogger("ReaderMiddleware");
        logger?.LogInformation("Starting document reading");

        await foreach (var doc in _reader.ReadAsync())
        {
            ctx.Documents.Add(doc);
        }

        logger?.LogInformation("Reading completed, loaded {DocumentCount} documents", ctx.Documents.Count);

        await next(ctx);
    }
}
