using Microsoft.Extensions.DataIngestion;

namespace AIKit.DataIngestion.Services.Processors;

public interface IIngestionDocumentProcessor
{
    Task<IngestionDocument> ProcessAsync(IngestionDocument ingestionDocument, CancellationToken ct);
}