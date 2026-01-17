namespace AIKit.Storage;

/// <summary>
/// Represents version information for a stored file.
/// </summary>
public sealed class StorageVersionInfo
{
    /// <summary>
    /// Gets the version identifier.
    /// </summary>
    public string Version { get; init; } = default!;

    /// <summary>
    /// Gets the size of the version in bytes, if available.
    /// </summary>
    public long? Size { get; init; }

    /// <summary>
    /// Gets the content type of the version, if available.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Gets the creation timestamp of the version.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets custom metadata associated with the version, if any.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; init; }
}