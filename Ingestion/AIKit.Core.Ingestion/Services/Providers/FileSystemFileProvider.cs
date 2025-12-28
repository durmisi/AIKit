using System.Runtime.CompilerServices;

namespace AIKit.Core.Ingestion.Services.Providers;

public sealed class FileSystemFileProvider : IIngestionFileProvider
{
    private readonly DirectoryInfo _directory;

    public FileSystemFileProvider(DirectoryInfo directory)
    {
        _directory = directory;
    }

    public async IAsyncEnumerable<IIngestionFile> ReadAsync(
       [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var files = _directory.EnumerateFiles("*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            yield return new LocalFile(file);
        }
    }
}