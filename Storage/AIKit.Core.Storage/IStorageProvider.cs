namespace AIKit.Storage;

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

    /// <summary>
    /// Checks if a file or a specific version exists.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="version">Optional version to check. If null, checks the latest.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    Task<bool> ExistsAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default);


    // -------- METADATA --------

    /// <summary>
    /// Gets metadata for a file or a specific version.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="version">Optional version to get metadata for. If null, gets for the latest.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The metadata if available; otherwise, null.</returns>
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



