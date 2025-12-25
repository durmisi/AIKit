namespace AIKit.Core.Ingestion.Services;

public interface IIngestionDocumentProvider
{
    IAsyncEnumerable<IngestionDocument> ReadAsync(
        CancellationToken cancellationToken = default);
}
