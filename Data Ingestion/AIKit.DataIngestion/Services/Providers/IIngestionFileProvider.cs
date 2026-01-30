namespace AIKit.Core.Ingestion.Services.Providers;

public interface IIngestionFileProvider
{
    IAsyncEnumerable<IIngestionFile> ReadAsync(
        CancellationToken cancellationToken = default);
}