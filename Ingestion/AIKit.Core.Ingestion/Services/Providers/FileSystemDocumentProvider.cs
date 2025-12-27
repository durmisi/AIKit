using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Providers;

public sealed class FileSystemDocumentProvider : IIngestionDocumentProvider
{
    private readonly DirectoryInfo _directory;
    private readonly Dictionary<string, IngestionDocumentReader> _readers;

    public FileSystemDocumentProvider(DirectoryInfo directory, Dictionary<string, IngestionDocumentReader> readers)
    {
        _directory = directory;
        _readers = readers ?? throw new ArgumentNullException(nameof(readers));
    }

    public async IAsyncEnumerable<IngestionDocument> ReadAsync(
        CancellationToken cancellationToken = default)
    {
        var files = _directory.EnumerateFiles("*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var extension = file.Extension.ToLowerInvariant();
            if (_readers.TryGetValue(extension, out var reader))
            {
                var ingestionDocument = await reader.ReadAsync(file, cancellationToken);
                yield return ingestionDocument;
            }
            // Skip files with unsupported extensions
        }
    }
}