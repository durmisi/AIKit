using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;

namespace AIKit.DataIngestion;

public sealed class DataIngestionContext : IngestionContext
{
    public IList<IngestionDocument> Documents { get; } = [];

    public IDictionary<string, IReadOnlyList<IngestionChunk<string>>> DocumentChunks { get; } = new Dictionary<string, IReadOnlyList<IngestionChunk<string>>>();

    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

    public ILoggerFactory? LoggerFactory { get; set; }
}