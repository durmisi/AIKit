using Microsoft.Extensions.DataIngestion;

namespace AIKit.DataIngestion.Services.Writers;

public interface IDocumentWriter
{
    Task WriteAsync(
        IngestionDocument ingestionDocument,
        IReadOnlyList<IngestionChunk<string>> chunks,
        CancellationToken cancellationToken);
}