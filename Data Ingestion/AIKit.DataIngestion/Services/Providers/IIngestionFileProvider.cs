namespace AIKit.DataIngestion.Services.Providers;

public interface IIngestionFileProvider
{
    IAsyncEnumerable<IIngestionFile> ReadAsync(
        CancellationToken cancellationToken = default);
}