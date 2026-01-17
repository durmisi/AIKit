namespace AIKit.Storage;

/// <summary>
/// Represents the result of a storage read operation.
/// </summary>
public sealed class StorageReadResult
{
    /// <summary>
    /// Gets the content stream of the read file.
    /// </summary>
    public Stream Content { get; }

    /// <summary>
    /// Gets the metadata of the read file.
    /// </summary>
    public StorageMetadata Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageReadResult"/> class.
    /// </summary>
    /// <param name="content">The content stream of the read file.</param>
    /// <param name="metadata">The metadata of the read file.</param>
    public StorageReadResult(Stream content, StorageMetadata metadata)
    {
        Content = content;
        Metadata = metadata;
    }
}