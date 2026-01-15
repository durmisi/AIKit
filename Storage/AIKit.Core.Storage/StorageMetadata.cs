namespace AIKit.Storage;

public sealed class StorageMetadata
{
    public string Path { get; init; } = default!;
    public string Version { get; init; } = default!;
    public long? Size { get; init; }
    public string? ContentType { get; init; }
    public IDictionary<string, string>? CustomMetadata { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}