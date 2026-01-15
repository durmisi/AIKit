namespace AIKit.Storage;

public sealed class StorageWriteOptions
{
    public string? ContentType { get; set; }

    /// <summary>
    /// Custom metadata key/value pairs.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// If true, creates a new version. Otherwise replaces the latest one.
    /// </summary>
    public bool CreateNewVersion { get; set; } = true;

    /// <summary>
    /// Optional version tag provided by the caller (manual version name).
    /// </summary>
    public string? VersionTag { get; set; }

    /// <summary>
    /// Optional overwrite protection.
    /// </summary>
    public bool OverwriteLatest { get; set; } = true;
}