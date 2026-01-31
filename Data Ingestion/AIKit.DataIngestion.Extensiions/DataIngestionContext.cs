using Microsoft.Extensions.DataIngestion;

namespace AIKit.DataIngestion;

public sealed class DataIngestionContext : IngestionContext
{
    public IList<IngestionDocument> Documents { get; } = [];

    public IDictionary<string, IReadOnlyList<IngestionChunk<string>>> DocumentChunks { get; } = new Dictionary<string, IReadOnlyList<IngestionChunk<string>>>();
}