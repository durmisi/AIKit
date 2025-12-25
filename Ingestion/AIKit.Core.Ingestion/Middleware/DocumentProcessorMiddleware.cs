using AIKit.Core.Ingestion.Services;

namespace AIKit.Core.Ingestion.Middleware;

public sealed class DocumentProcessorMiddleware : IIngestionMiddleware<DataIngestionContext>
{
    private readonly IEnumerable<IDocumentProcessor> _processors;

    public DocumentProcessorMiddleware(IEnumerable<IDocumentProcessor> processors)
    {
        _processors = processors;
    }

    public async Task InvokeAsync(
        DataIngestionContext ctx,
        IngestionDelegate<DataIngestionContext> next)
    {
        foreach (var doc in ctx.Documents)
        {
            foreach (var processor in _processors)
            {
                await processor.ProcessAsync(doc, CancellationToken.None);
            }
        }

        await next(ctx);
    }
}
