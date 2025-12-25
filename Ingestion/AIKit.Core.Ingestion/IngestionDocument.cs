namespace AIKit.Core.Ingestion;

public sealed class IngestionDocument
{
    public required string Id { get; init; }
    public required string Content { get; set; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
}
