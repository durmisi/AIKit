namespace AIKit.Storage;

/// <summary>
/// Represents metadata for a stored file.
/// </summary>
public sealed class StorageMetadata
{
    /// <summary>
    /// Gets the path of the file.
    /// </summary>
    public string Path { get; init; } = default!;

    /// <summary>
    /// Gets the version of the file.
    /// </summary>
    public string Version { get; init; } = default!;

    /// <summary>
    /// Gets the size of the file in bytes, if available.
    /// </summary>
    public long? Size { get; init; }

    /// <summary>
    /// Gets the content type of the file, if available.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets custom metadata associated with the file, if any.
    /// </summary>
    public IDictionary<string, string>? CustomMetadata { get; init; }

    /// <summary>
    /// Gets the creation timestamp of the file, if available.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; init; }
}