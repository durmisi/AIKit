namespace AIKit.Core.Storage;

public interface IStorageProvider
{
    // -------- SAVE / WRITE --------

    /// <summary>
    /// Stores a file and returns the generated (or updated) version info.
    /// </summary>
    Task<StorageWriteResult> SaveAsync(
        string path,
        Stream content,
        StorageWriteOptions? options = null,
        CancellationToken cancellationToken = default);


    // -------- READ --------

    /// <summary>
    /// Reads a specific file version. If version is null, reads the latest.
    /// </summary>
    Task<StorageReadResult?> ReadAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default);


    // -------- DELETE --------

    /// <summary>
    /// Deletes a file or a specific version (depending on provider capabilities).
    /// </summary>
    Task<bool> DeleteAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default);


    // -------- EXISTS --------

    Task<bool> ExistsAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default);


    // -------- METADATA --------

    Task<StorageMetadata?> GetMetadataAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default);


    // -------- VERSIONING --------

    /// <summary>
    /// Returns all versions of a file, latest first.
    /// </summary>
    Task<IReadOnlyList<StorageVersionInfo>> ListVersionsAsync(
        string path,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Restores a previous version as the new latest version.
    /// </summary>
    Task<StorageWriteResult> RestoreVersionAsync(
        string path,
        string version,
        CancellationToken cancellationToken = default);
}


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



public sealed class StorageWriteResult
{
    public string Path { get; }
    public string Version { get; }
    public long Size { get; }
    public IDictionary<string, string>? Metadata { get; }

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



public sealed class StorageMetadata
{
    public string Path { get; init; } = default!;
    public string Version { get; init; } = default!;
    public long? Size { get; init; }
    public string? ContentType { get; init; }
    public IDictionary<string, string>? CustomMetadata { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}



public sealed class StorageVersionInfo
{
    public string Version { get; init; } = default!;
    public long? Size { get; init; }
    public string? ContentType { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
}



