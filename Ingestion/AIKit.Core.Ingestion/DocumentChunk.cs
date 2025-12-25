namespace AIKit.Core.Ingestion;

public sealed class DocumentChunk
{
    public required string DocumentId { get; init; }
    public required string Content { get; init; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();
}
