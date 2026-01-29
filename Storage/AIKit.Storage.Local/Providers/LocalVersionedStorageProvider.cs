using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIKit.Storage.Local;

/// <summary>
/// Local file system storage provider with versioning support.
/// Uses folder structure: {basePath}/{filePath}/{version}/content.bin
/// Metadata stored alongside as metadata.json
/// </summary>
public sealed class LocalVersionedStorageProvider : IStorageProvider
{
    private const string ContentFileName = "content.bin";
    private const string MetadataFileName = "metadata.json";

    private readonly string _basePath;
    private readonly ILogger<LocalVersionedStorageProvider>? _logger;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LocalVersionedStorageProvider(string basePath, ILogger<LocalVersionedStorageProvider>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);
        _basePath = Path.GetFullPath(basePath);
        _logger = logger;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<StorageWriteResult> SaveAsync(
        string path,
        Stream content,
        StorageWriteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(content);

        options ??= new StorageWriteOptions();

        _logger?.LogInformation("Starting save operation for file {Path} with write mode {WriteMode}", path, options.WriteMode);

        var fileFolderPath = GetFileFolderPath(path);

        // Handle write mode logic
        string version;
        switch (options.WriteMode)
        {
            case StorageWriteMode.FailIfExists:
                if (await ExistsAsync(path, cancellationToken: cancellationToken))
                {
                    _logger?.LogWarning("File {Path} already exists, failing as per FailIfExists mode", path);
                    throw new InvalidOperationException($"File '{path}' already exists.");
                }
                version = GenerateVersion(options.VersionTag);
                _logger?.LogInformation("Generated new version {Version} for FailIfExists mode", version);
                break;
            case StorageWriteMode.ReplaceLatest:
                var latestVersion = await GetLatestVersionAsync(path, cancellationToken);
                version = latestVersion ?? GenerateVersion(options.VersionTag);
                _logger?.LogInformation("Using version {Version} for ReplaceLatest mode (latest was {Latest})", version, latestVersion ?? "none");
                break;
            case StorageWriteMode.CreateNewVersion:
            default:
                version = GenerateVersion(options.VersionTag);
                _logger?.LogInformation("Generated new version {Version} for CreateNewVersion mode", version);
                break;
        }

        var versionFolderPath = Path.Combine(fileFolderPath, version);

        Directory.CreateDirectory(versionFolderPath);
        _logger?.LogDebug("Created directory {VersionFolderPath}", versionFolderPath);

        var contentFilePath = Path.Combine(versionFolderPath, ContentFileName);
        var tempContentFilePath = contentFilePath + ".tmp";
        var metadataFilePath = Path.Combine(versionFolderPath, MetadataFileName);
        var tempMetadataFilePath = metadataFilePath + ".tmp";

        _logger?.LogInformation("Saving file {Path} version {Version}", path, version);

        try
        {
            // Write content to temp file
            _logger?.LogDebug("Writing content to temp file {TempContentFilePath}", tempContentFilePath);
            await using (var fileStream = new FileStream(tempContentFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            var fileInfo = new FileInfo(tempContentFilePath);
            _logger?.LogDebug("Content written, size: {Size} bytes", fileInfo.Length);

            // Write metadata to temp file
            _logger?.LogDebug("Writing metadata to temp file {TempMetadataFilePath}", tempMetadataFilePath);
            var metadata = new LocalStorageMetadata
            {
                Path = path,
                Version = version,
                Size = fileInfo.Length,
                ContentType = options.ContentType,
                CustomMetadata = options.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                CreatedAt = DateTimeOffset.UtcNow
            };

            var metadataJson = JsonSerializer.Serialize(metadata, _jsonOptions);
            await File.WriteAllTextAsync(tempMetadataFilePath, metadataJson, cancellationToken);
            _logger?.LogDebug("Metadata written");

            // Atomic move
            _logger?.LogDebug("Performing atomic move for content file");
            File.Move(tempContentFilePath, contentFilePath, overwrite: true);
            _logger?.LogDebug("Performing atomic move for metadata file");
            File.Move(tempMetadataFilePath, metadataFilePath, overwrite: true);

            _logger?.LogInformation("Successfully saved file {Path} version {Version} with size {Size}", path, version, fileInfo.Length);

            return new StorageWriteResult(path, version, fileInfo.Length, options.Metadata);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save file {Path} version {Version}", path, version);
            // Clean up temp files
            try { File.Delete(tempContentFilePath); } catch { }
            try { File.Delete(tempMetadataFilePath); } catch { }
            throw;
        }
    }

    public async Task<StorageReadResult?> ReadAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Reading file {Path} version {Version}", path, version ?? "latest");

        version ??= await GetLatestVersionAsync(path, cancellationToken);
        if (version is null)
        {
            _logger?.LogWarning("No versions found for file {Path}", path);
            return null;
        }

        var versionFolderPath = Path.Combine(GetFileFolderPath(path), version);
        var contentFilePath = Path.Combine(versionFolderPath, ContentFileName);
        var metadataFilePath = Path.Combine(versionFolderPath, MetadataFileName);

        if (!File.Exists(contentFilePath))
        {
            _logger?.LogWarning("Content file not found for {Path} version {Version}", path, version);
            return null;
        }

        var metadata = await ReadMetadataFileAsync(metadataFilePath, cancellationToken);
        if (metadata is null)
        {
            _logger?.LogWarning("Metadata not found or invalid for {Path} version {Version}", path, version);
            return null;
        }

        var stream = new FileStream(contentFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

        _logger?.LogInformation("Successfully read file {Path} version {Version}", path, version);

        return new StorageReadResult(stream, new StorageMetadata
        {
            Path = metadata.Path,
            Version = metadata.Version,
            Size = metadata.Size,
            ContentType = metadata.ContentType,
            CustomMetadata = metadata.CustomMetadata,
            CreatedAt = metadata.CreatedAt
        });
    }

    public async Task<bool> DeleteAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Deleting file {Path} version {Version}", path, version);

        var fileFolderPath = GetFileFolderPath(path);

        if (version is not null)
        {
            // Delete specific version
            var versionFolderPath = Path.Combine(fileFolderPath, version);
            if (!Directory.Exists(versionFolderPath))
                return false;

            Directory.Delete(versionFolderPath, recursive: true);

            // Clean up parent if empty
            if (Directory.Exists(fileFolderPath) && !Directory.EnumerateFileSystemEntries(fileFolderPath).Any())
            {
                Directory.Delete(fileFolderPath);
            }

            _logger?.LogInformation("Successfully deleted file {Path} version {Version}", path, version);

            return true;
        }

        // Delete all versions
        if (!Directory.Exists(fileFolderPath))
            return false;

        Directory.Delete(fileFolderPath, recursive: true);

        _logger?.LogInformation("Successfully deleted all versions of file {Path}", path);

        return true;
    }

    public async Task<bool> ExistsAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Checking existence of file {Path} version {Version}", path, version ?? "latest");

        version ??= await GetLatestVersionAsync(path, cancellationToken);
        if (version is null)
        {
            _logger?.LogInformation("File {Path} does not exist", path);
            return false;
        }

        var contentFilePath = Path.Combine(GetFileFolderPath(path), version, ContentFileName);
        var exists = File.Exists(contentFilePath);

        _logger?.LogInformation("File {Path} version {Version} exists: {Exists}", path, version, exists);

        return exists;
    }

    public async Task<StorageMetadata?> GetMetadataAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Getting metadata for file {Path} version {Version}", path, version ?? "latest");

        version ??= await GetLatestVersionAsync(path, cancellationToken);
        if (version is null)
        {
            _logger?.LogWarning("No versions found for file {Path}", path);
            return null;
        }

        var metadataFilePath = Path.Combine(GetFileFolderPath(path), version, MetadataFileName);
        var metadata = await ReadMetadataFileAsync(metadataFilePath, cancellationToken);

        if (metadata is null)
        {
            _logger?.LogWarning("Metadata not found or invalid for {Path} version {Version}", path, version);
            return null;
        }

        _logger?.LogInformation("Successfully retrieved metadata for {Path} version {Version}", path, version);

        return new StorageMetadata
        {
            Path = metadata.Path,
            Version = metadata.Version,
            Size = metadata.Size,
            ContentType = metadata.ContentType,
            CustomMetadata = metadata.CustomMetadata,
            CreatedAt = metadata.CreatedAt
        };
    }

    public Task<IReadOnlyList<StorageVersionInfo>> ListVersionsAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Listing versions for file {Path}", path);

        var fileFolderPath = GetFileFolderPath(path);
        if (!Directory.Exists(fileFolderPath))
        {
            _logger?.LogInformation("No versions found for file {Path}", path);
            return Task.FromResult<IReadOnlyList<StorageVersionInfo>>([]);
        }

        var versions = new List<StorageVersionInfo>();

        foreach (var versionDir in Directory.EnumerateDirectories(fileFolderPath))
        {
            var metadataFilePath = Path.Combine(versionDir, MetadataFileName);
            if (!File.Exists(metadataFilePath))
                continue;

            try
            {
                var json = File.ReadAllText(metadataFilePath);
                var metadata = JsonSerializer.Deserialize<LocalStorageMetadata>(json, _jsonOptions);
                if (metadata is null)
                    continue;

                versions.Add(new StorageVersionInfo
                {
                    Version = metadata.Version,
                    Size = metadata.Size,
                    ContentType = metadata.ContentType,
                    CreatedAt = metadata.CreatedAt,
                    Metadata = metadata.CustomMetadata
                });
            }
            catch (JsonException)
            {
                // Skip invalid metadata files
            }
        }

        // Return versions sorted by creation time (latest first)
        var result = versions
            .OrderByDescending(v => v.CreatedAt)
            .ToList();

        _logger?.LogInformation("Found {Count} versions for file {Path}", result.Count, path);

        return Task.FromResult<IReadOnlyList<StorageVersionInfo>>(result);
    }

    public async Task<StorageWriteResult> RestoreVersionAsync(
        string path,
        string version,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        _logger?.LogInformation("Restoring file {Path} to version {Version}", path, version);

        var readResult = await ReadAsync(path, version, cancellationToken)
            ?? throw new InvalidOperationException($"Version '{version}' not found for path '{path}'.");

        await using var memoryStream = new MemoryStream();
        await using (readResult.Content)
        {
            await readResult.Content.CopyToAsync(memoryStream, cancellationToken);
        }
        memoryStream.Position = 0;

        var options = new StorageWriteOptions
        {
            ContentType = readResult.Metadata.ContentType,
            Metadata = readResult.Metadata.CustomMetadata,
            WriteMode = StorageWriteMode.CreateNewVersion
        };

        return await SaveAsync(path, memoryStream, options, cancellationToken);
    }

    private string GetFileFolderPath(string path)
    {
        // Sanitize the path to be safe for file system
        var sanitizedPath = SanitizePath(path);
        return Path.Combine(_basePath, sanitizedPath);
    }

    private static string SanitizePath(string path)
    {
        // Replace invalid characters and normalize separators
        var invalidChars = Path.GetInvalidPathChars().Concat([':', '*', '?', '"', '<', '>', '|']).ToArray();
        var sanitized = path;

        foreach (var c in invalidChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }

        // Normalize path separators
        sanitized = sanitized.Replace('/', Path.DirectorySeparatorChar);
        sanitized = sanitized.Replace('\\', Path.DirectorySeparatorChar);

        return sanitized.Trim(Path.DirectorySeparatorChar);
    }

    private static string GenerateVersion(string? versionTag)
    {
        if (!string.IsNullOrWhiteSpace(versionTag))
        {
            // Sanitize version tag
            var sanitized = SanitizePath(versionTag);
            return $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}_{sanitized}";
        }

        return DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff");
    }

    private async Task<string?> GetLatestVersionAsync(string path, CancellationToken cancellationToken)
    {
        var versions = await ListVersionsAsync(path, cancellationToken);
        return versions.FirstOrDefault()?.Version;
    }

    private async Task<LocalStorageMetadata?> ReadMetadataFileAsync(string metadataFilePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(metadataFilePath))
            return null;

        try
        {
            var json = await File.ReadAllTextAsync(metadataFilePath, cancellationToken);
            return JsonSerializer.Deserialize<LocalStorageMetadata>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            _logger?.LogWarning(ex, "Failed to deserialize metadata from {Path}", metadataFilePath);
            return null;
        }
    }

    private sealed class LocalStorageMetadata
    {
        public string Path { get; set; } = default!;
        public string Version { get; set; } = default!;
        public long? Size { get; set; }
        public string? ContentType { get; set; }
        public IDictionary<string, string>? CustomMetadata { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}