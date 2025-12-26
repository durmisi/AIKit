using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Providers;

public sealed class FileSystemDocumentProvider : IIngestionDocumentProvider
{
    private readonly DirectoryInfo _directory;
    private readonly string _searchPattern;

    public FileSystemDocumentProvider(DirectoryInfo directory, string searchPattern = "*.*")
    {
        _directory = directory;
        _searchPattern = searchPattern;
    }

    public async IAsyncEnumerable<IngestionDocument> ReadAsync(
        CancellationToken cancellationToken = default)
    {
        var files = _directory.EnumerateFiles(_searchPattern, SearchOption.AllDirectories);

        var reader = new MarkdownReader();

        foreach (var file in files)
        {
            var ingestionDocument = await reader.ReadAsync(file, cancellationToken);

            yield return ingestionDocument;
        }
    }
}