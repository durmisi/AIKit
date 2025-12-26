using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Providers;

public interface IIngestionDocumentProvider
{
    IAsyncEnumerable<IngestionDocument> ReadAsync(
        CancellationToken cancellationToken = default);
}
