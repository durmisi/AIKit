namespace AIKit.Storage;

/// <summary>
/// Represents the result of a storage write operation.
/// </summary>
public sealed class StorageWriteResult
{
    /// <summary>
    /// Gets the path of the written file.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the version of the written file.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the size of the written file in bytes.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Gets any metadata associated with the write operation.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageWriteResult"/> class.
    /// </summary>
    /// <param name="path">The path of the written file.</param>
    /// <param name="version">The version of the written file.</param>
    /// <param name="size">The size of the written file in bytes.</param>
    /// <param name="metadata">Optional metadata associated with the write operation.</param>
    public StorageWriteResult(
        string path,
        string version,
        long size,
        IDictionary<string, string>? metadata = null)
    {
        Path = path;
        Version = version;
        Size = size;
        Metadata = metadata;
    }
}