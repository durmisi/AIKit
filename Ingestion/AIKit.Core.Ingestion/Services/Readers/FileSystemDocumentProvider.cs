using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Readers;

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
            var id = Path.GetFileNameWithoutExtension(file.Name);

            IngestionDocument document;
            if (file.Extension.Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                var reader = new MarkdownReader();
                var mdFile = await reader.ReadAsync(new FileInfo(file.FullName), id);

                var content = string.Join(
                    Environment.NewLine,
                    mdFile.EnumerateContent().Select(x => x.Text)
                );

                document = new IngestionDocument
                {
                    Id = id,
                    Content = content ?? "",
                    Metadata = new Dictionary<string, object>
                    {
                        ["filePath"] = file.FullName,
                        ["fileExtension"] = file.Extension
                    }
                };
            }
            else
            {
                var content = await File.ReadAllTextAsync(file.FullName, cancellationToken);

                document = new IngestionDocument
                {
                    Id = id,
                    Content = content,
                    Metadata = new Dictionary<string, object>
                    {
                        ["filePath"] = file.FullName,
                        ["fileExtension"] = file.Extension
                    }
                };
            }

            yield return document;
        }
    }
}