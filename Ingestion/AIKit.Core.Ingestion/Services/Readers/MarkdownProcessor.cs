using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Readers;

public sealed class MarkdownReaderEx : IDocumentReader
{
    private readonly MarkdownReader _reader;

    public MarkdownReaderEx()
    {
        _reader = new MarkdownReader();
    }

    public async Task<IngestionDocument> ReadAsync(string filePath, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}