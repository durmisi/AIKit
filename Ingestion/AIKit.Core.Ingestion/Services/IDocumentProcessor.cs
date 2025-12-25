namespace AIKit.Core.Ingestion.Services;

public interface IDocumentProcessor
{
    Task ProcessAsync(IngestionDocument document, CancellationToken ct);
}
