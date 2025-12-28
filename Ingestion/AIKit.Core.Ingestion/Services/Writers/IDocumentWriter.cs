using Microsoft.Extensions.DataIngestion;

namespace AIKit.Core.Ingestion.Services.Writers;

public interface IDocumentWriter
{
    Task WriteAsync(
        IngestionDocument ingestionDocument,
        IReadOnlyList<IngestionChunk<string>> chunks,
        CancellationToken cancellationToken);
}
