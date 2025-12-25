namespace AIKit.Core.Ingestion.Services.Writers;

public interface IDocumentWriter
{
    Task WriteAsync(
        IReadOnlyList<DocumentChunk> chunks,
        CancellationToken cancellationToken);
}
