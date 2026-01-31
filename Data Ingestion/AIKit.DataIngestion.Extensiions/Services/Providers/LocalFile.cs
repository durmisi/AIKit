namespace AIKit.DataIngestion.Services.Providers;

public sealed class LocalFile : IIngestionFile
{
    private readonly FileInfo _fileInfo;

    public LocalFile(FileInfo fileInfo)
    {
        _fileInfo = fileInfo;
    }

    public string Name => _fileInfo.Name;
    public string Extension => _fileInfo.Extension;

    public Task<Stream> OpenReadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Stream>(_fileInfo.OpenRead());
    }
}