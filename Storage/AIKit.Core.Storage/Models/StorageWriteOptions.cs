namespace AIKit.Storage;


public sealed class StorageWriteOptions
{
    /// <summary>
    /// Gets or sets the content type of the file.
    /// </summary>
    public string? ContentType { get; init; }

    /// <summary>
    /// Custom metadata key/value pairs.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Determines how the write operation behaves.
    /// </summary>
    public StorageWriteMode WriteMode { get; init; }
        = StorageWriteMode.CreateNewVersion;

    /// <summary>
    /// Optional logical version label (e.g. "original", "ocr-v1", "embeddings-v2").
    /// Applies only when creating a new version.
    /// </summary>
    public string? VersionTag { get; init; }
}

public enum StorageWriteMode
{
    /// <summary>
    /// Always create a new version and make it the latest.
    /// Fails only if the provider does not support versioning.
    /// </summary>
    CreateNewVersion,

    /// <summary>
    /// Replace the latest version (no new version created).
    /// </summary>
    ReplaceLatest,

    /// <summary>
    /// Fail if the file (or latest version) already exists.
    /// </summary>
    FailIfExists
}