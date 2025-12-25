namespace AIKit.Core.Ingestion;

public sealed class DocumentChunk
{
    public required string DocumentId { get; init; }
    public required string Content { get; init; }
    public required IDictionary<string, object> Metadata { get; init; }
}
