namespace AIKit.Core.Ingestion.Services.Providers;

public interface IIngestionFileProvider
{
    IAsyncEnumerable<FileInfo> ReadAsync(
        CancellationToken cancellationToken = default);
}
