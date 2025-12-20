namespace AIKit.Core.Storage;

public sealed class StorageReadResult
{
    public Stream Content { get; }
    public StorageMetadata Metadata { get; }

    public StorageReadResult(Stream content, StorageMetadata metadata)
    {
        Content = content;
        Metadata = metadata;
    }
}