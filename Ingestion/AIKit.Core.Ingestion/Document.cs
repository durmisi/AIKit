namespace AIKit.Core.Ingestion;

public sealed class Document
{
    public required string Id { get; init; }
    public required string Content { get; set; }
    public required IDictionary<string, object> Metadata { get; init; }
}
