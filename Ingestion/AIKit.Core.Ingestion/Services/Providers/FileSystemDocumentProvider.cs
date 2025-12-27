using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Providers;

public sealed class FileSystemDocumentProvider : IIngestionDocumentProvider
{
    private readonly DirectoryInfo _directory;
    private readonly string _searchPattern;
    private readonly IngestionDocumentReader _reader;

    public FileSystemDocumentProvider(DirectoryInfo directory, IngestionDocumentReader reader, string searchPattern = "*.*")
    {
        _directory = directory;
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _searchPattern = searchPattern;
    }

    public async IAsyncEnumerable<IngestionDocument> ReadAsync(
        CancellationToken cancellationToken = default)
    {
        var files = _directory.EnumerateFiles(_searchPattern, SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var ingestionDocument = await _reader.ReadAsync(file, cancellationToken);

            yield return ingestionDocument;
        }
    }
}