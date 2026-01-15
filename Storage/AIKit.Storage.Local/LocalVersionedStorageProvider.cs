using AIKit.Core.Storage;
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

        var fileFolderPath = GetFileFolderPath(path);
        var version = GenerateVersion(options.VersionTag);
        var versionFolderPath = Path.Combine(fileFolderPath, version);

        // Handle overwrite logic
        if (!options.CreateNewVersion && options.OverwriteLatest)
        {
            var latestVersion = await GetLatestVersionAsync(path, cancellationToken);
            if (latestVersion is not null)
            {
                versionFolderPath = Path.Combine(fileFolderPath, latestVersion);
                version = latestVersion;
            }
        }

        Directory.CreateDirectory(versionFolderPath);

        var contentFilePath = Path.Combine(versionFolderPath, ContentFileName);
        var tempContentFilePath = contentFilePath + ".tmp";
        var metadataFilePath = Path.Combine(versionFolderPath, MetadataFileName);
        var tempMetadataFilePath = metadataFilePath + ".tmp";

        _logger?.LogInformation("Saving file {Path} version {Version}", path, version);

        try
        {
            // Write content to temp file
            await using (var fileStream = new FileStream(tempContentFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
            }

            var fileInfo = new FileInfo(tempContentFilePath);

            // Write metadata to temp file
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

            // Atomic move
            File.Move(tempContentFilePath, contentFilePath, overwrite: true);
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

        version ??= await GetLatestVersionAsync(path, cancellationToken);
        if (version is null)
            return null;

        var versionFolderPath = Path.Combine(GetFileFolderPath(path), version);
        var contentFilePath = Path.Combine(versionFolderPath, ContentFileName);
        var metadataFilePath = Path.Combine(versionFolderPath, MetadataFileName);

        if (!File.Exists(contentFilePath))
            return null;

        var metadata = await ReadMetadataFileAsync(metadataFilePath, cancellationToken);
        if (metadata is null)
            return null;

        var stream = new FileStream(contentFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);

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

            return true;
        }

        // Delete all versions
        if (!Directory.Exists(fileFolderPath))
            return false;

        Directory.Delete(fileFolderPath, recursive: true);
        return true;
    }

    public async Task<bool> ExistsAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        version ??= await GetLatestVersionAsync(path, cancellationToken);
        if (version is null)
            return false;

        var contentFilePath = Path.Combine(GetFileFolderPath(path), version, ContentFileName);
        return File.Exists(contentFilePath);
    }

    public async Task<StorageMetadata?> GetMetadataAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        version ??= await GetLatestVersionAsync(path, cancellationToken);
        if (version is null)
            return null;

        var metadataFilePath = Path.Combine(GetFileFolderPath(path), version, MetadataFileName);
        var metadata = await ReadMetadataFileAsync(metadataFilePath, cancellationToken);

        if (metadata is null)
            return null;

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

        var fileFolderPath = GetFileFolderPath(path);
        if (!Directory.Exists(fileFolderPath))
            return Task.FromResult<IReadOnlyList<StorageVersionInfo>>([]);

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
                    CreatedAt = metadata.CreatedAt ?? DateTimeOffset.MinValue,
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

        return Task.FromResult<IReadOnlyList<StorageVersionInfo>>(result);
    }

    public async Task<StorageWriteResult> RestoreVersionAsync(
        string path,
        string version,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        var readResult = await ReadAsync(path, version, cancellationToken)
            ?? throw new InvalidOperationException($"Version '{version}' not found for path '{path}'.");

        await using (readResult.Content)
        {
            var options = new StorageWriteOptions
            {
                ContentType = readResult.Metadata.ContentType,
                Metadata = readResult.Metadata.CustomMetadata,
                CreateNewVersion = true
            };

            return await SaveAsync(path, readResult.Content, options, cancellationToken);
        }
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
        catch (JsonException)
        {
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
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
