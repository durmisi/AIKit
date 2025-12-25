namespace AIKit.Core.Ingestion;

public sealed class DataIngestionContext : IngestionContext
{
    public IList<IngestionDocument> Documents { get; } = [];
    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();
}
