using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Processors;

public interface IIngestionDocumentProcessor
{
    Task<IngestionDocument> ProcessAsync(IngestionDocument ingestionDocument, CancellationToken ct);
}