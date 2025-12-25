namespace AIKit.Core.Ingestion.Services.Processors;

public interface IDocumentProcessor
{
    Task ProcessAsync(IngestionDocument document, CancellationToken ct);
}
