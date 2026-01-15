namespace AIKit.Storage;

public sealed class StorageVersionInfo
{
    public string Version { get; init; } = default!;
    public long? Size { get; init; }
    public string? ContentType { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
}