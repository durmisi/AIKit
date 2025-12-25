namespace AIKit.Core.Ingestion.Services.Readers;

public interface IIngestionDocumentProvider
{
    IAsyncEnumerable<IngestionDocument> ReadAsync(
        CancellationToken cancellationToken = default);
}
