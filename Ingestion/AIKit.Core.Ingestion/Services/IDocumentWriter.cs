namespace AIKit.Core.Ingestion.Services;

public interface IDocumentWriter
{
    Task WriteAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken);
}
