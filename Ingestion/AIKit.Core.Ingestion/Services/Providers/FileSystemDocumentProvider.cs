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

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file.FullName, cancellationToken);
            var id = Path.GetFileNameWithoutExtension(file.Name);

            var metadata = new Dictionary<string, object>
            {
                ["filePath"] = file.FullName,
                ["fileExtension"] = file.Extension
            };

            yield return new IngestionDocument
            {
                Id = id,
                Content = content,
                Metadata = metadata
            };
        }
    }
}