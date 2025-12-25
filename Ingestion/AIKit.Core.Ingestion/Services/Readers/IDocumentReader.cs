namespace AIKit.Core.Ingestion.Services.Readers;

public interface IDocumentReader
{
    Task<IngestionDocument> ReadAsync(string filePath, CancellationToken ct);
}
