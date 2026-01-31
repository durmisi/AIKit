namespace AIKit.DataIngestion.Services.Providers;

public interface IIngestionFile
{
    string Name { get; }
    string Extension { get; }

    Task<Stream> OpenReadAsync(CancellationToken cancellationToken = default);
}