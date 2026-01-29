using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

namespace AIKit.Storage.Azure;

/// <summary>
/// Azure Blob Storage provider with native blob versioning support.
/// Supports metadata merging and optional SAS URL generation.
/// </summary>
public sealed class AzureBlobStorageProvider : IStorageProvider
{
    private const string MetadataPrefix = "custom_";

    private readonly BlobContainerClient _containerClient;
    private readonly AzureBlobStorageOptions _options;
    private readonly ILogger<AzureBlobStorageProvider>? _logger;

    public AzureBlobStorageProvider(BlobContainerClient containerClient, AzureBlobStorageOptions? options = null, ILogger<AzureBlobStorageProvider>? logger = null)
    {
        _containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
        _options = options ?? new AzureBlobStorageOptions();
        _logger = logger;
    }

    public AzureBlobStorageProvider(string connectionString, string containerName, AzureBlobStorageOptions? options = null, ILogger<AzureBlobStorageProvider>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

        // Force a version compatible with Azurite
        var blobOptions = new BlobClientOptions(BlobClientOptions.ServiceVersion.V2023_11_03);

        var serviceClient = new BlobServiceClient(connectionString, blobOptions);
        _containerClient = serviceClient.GetBlobContainerClient(containerName);

        _options = options ?? new AzureBlobStorageOptions();
        _logger = logger;
    }

    /// <summary>
    /// Ensures the container exists with versioning enabled.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Initializing Azure Blob Storage container {ContainerName}", _containerClient.Name);
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        _logger?.LogInformation("Azure Blob Storage container {ContainerName} initialized", _containerClient.Name);
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

        var blobClient = _containerClient.GetBlobClient(NormalizePath(path));

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = options.ContentType ?? "application/octet-stream"
            },
            Metadata = BuildMetadata(options.Metadata, options.VersionTag),
            Conditions = options.WriteMode switch
            {
                StorageWriteMode.FailIfExists => new BlobRequestConditions { IfNoneMatch = ETag.All },
                _ => null
            }
        };

        var response = await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

        // Get the version ID from response (requires blob versioning enabled on storage account)
        var versionId = response.Value.VersionId ?? DateTimeOffset.UtcNow.ToString("o");

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        _logger?.LogInformation("Successfully saved file {Path} version {Version} with size {Size}", path, versionId, properties.Value.ContentLength);

        return new StorageWriteResult(
            path,
            versionId,
            properties.Value.ContentLength,
            ExtractCustomMetadata(properties.Value.Metadata));
    }

    public async Task<StorageReadResult?> ReadAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Reading file {Path} version {Version}", path, version ?? "latest");

        try
        {
            var blobClient = GetBlobClient(path, version);
            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

            var metadata = new StorageMetadata
            {
                Path = path,
                Version = version ?? response.Value.Details.VersionId ?? "current",
                Size = response.Value.Details.ContentLength,
                ContentType = response.Value.Details.ContentType,
                CustomMetadata = ExtractCustomMetadata(response.Value.Details.Metadata),
                CreatedAt = response.Value.Details.LastModified
            };

            return new StorageReadResult(response.Value.Content, metadata);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger?.LogWarning("File {Path} version {Version} not found", path, version ?? "latest");
            return null;
        }
    }

    public async Task<bool> DeleteAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Deleting file {Path} version {Version}", path, version);

        try
        {
            var blobClient = GetBlobClient(path, version);

            if (version is not null)
            {
                // Delete specific version
                await blobClient.DeleteAsync(DeleteSnapshotsOption.None, cancellationToken: cancellationToken);
            }
            else
            {
                // Delete blob and all versions/snapshots
                await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
            }

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger?.LogWarning("File {Path} version {Version} not found for deletion", path, version);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Checking existence of file {Path} version {Version}", path, version ?? "latest");

        try
        {
            var blobClient = GetBlobClient(path, version);
            var exists = await blobClient.ExistsAsync(cancellationToken);
            _logger?.LogInformation("File {Path} version {Version} exists: {Exists}", path, version ?? "latest", exists);
            return exists;
        }
        catch (RequestFailedException)
        {
            _logger?.LogInformation("File {Path} version {Version} does not exist", path, version ?? "latest");
            return false;
        }
    }

    public async Task<StorageMetadata?> GetMetadataAsync(
        string path,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Getting metadata for file {Path} version {Version}", path, version ?? "latest");

        try
        {
            var blobClient = GetBlobClient(path, version);
            var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

            return new StorageMetadata
            {
                Path = path,
                Version = version ?? properties.Value.VersionId ?? "current",
                Size = properties.Value.ContentLength,
                ContentType = properties.Value.ContentType,
                CustomMetadata = ExtractCustomMetadata(properties.Value.Metadata),
                CreatedAt = properties.Value.CreatedOn
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger?.LogWarning("Metadata not found for file {Path} version {Version}", path, version ?? "latest");
            return null;
        }
    }

    public async Task<IReadOnlyList<StorageVersionInfo>> ListVersionsAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogInformation("Listing versions for file {Path}", path);

        var normalizedPath = NormalizePath(path);
        var versions = new List<StorageVersionInfo>();

        await foreach (var blobItem in _containerClient.GetBlobsAsync(
            BlobTraits.Metadata,
            BlobStates.Version,
            prefix: normalizedPath,
            cancellationToken: cancellationToken))
        {
            // Only include exact path matches
            if (!string.Equals(blobItem.Name, normalizedPath, StringComparison.OrdinalIgnoreCase))
                continue;

            versions.Add(new StorageVersionInfo
            {
                Version = blobItem.VersionId ?? "current",
                Size = blobItem.Properties.ContentLength,
                ContentType = blobItem.Properties.ContentType,
                CreatedAt = blobItem.Properties.CreatedOn ?? DateTimeOffset.MinValue,
                Metadata = ExtractCustomMetadata(blobItem.Metadata)
            });
        }

        // Return versions sorted by creation time (latest first)
        var result = versions
            .OrderByDescending(v => v.CreatedAt)
            .ToList();

        _logger?.LogInformation("Found {Count} versions for file {Path}", result.Count, path);

        return result;
    }

    public async Task<StorageWriteResult> RestoreVersionAsync(
        string path,
        string version,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        _logger?.LogInformation("Restoring version {Version} for file {Path}", version, path);

        var normalizedPath = NormalizePath(path);

        // Get the versioned blob client
        var sourceBlobClient = _containerClient.GetBlobClient(normalizedPath).WithVersion(version);

        // Check if version exists
        if (!await sourceBlobClient.ExistsAsync(cancellationToken))
        {
            throw new InvalidOperationException($"Version '{version}' not found for path '{path}'.");
        }

        // Copy the version to create a new current version
        var destinationBlobClient = _containerClient.GetBlobClient(normalizedPath);

        var copyOperation = await destinationBlobClient.StartCopyFromUriAsync(
            sourceBlobClient.Uri,
            cancellationToken: cancellationToken);

        await copyOperation.WaitForCompletionAsync(cancellationToken);

        var properties = await destinationBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        _logger?.LogInformation("Successfully restored version {Version} for file {Path} as new version {NewVersion}", version, path, properties.Value.VersionId ?? DateTimeOffset.UtcNow.ToString("o"));

        return new StorageWriteResult(
            path,
            properties.Value.VersionId ?? DateTimeOffset.UtcNow.ToString("o"),
            properties.Value.ContentLength,
            ExtractCustomMetadata(properties.Value.Metadata));
    }

    /// <summary>
    /// Generates a SAS URL for accessing a blob.
    /// </summary>
    /// <param name="path">The blob path.</param>
    /// <param name="version">Optional version ID.</param>
    /// <param name="expiresIn">How long the SAS token is valid. Defaults to options value.</param>
    /// <param name="permissions">Permissions for the SAS token. Defaults to Read.</param>
    /// <returns>The SAS URL or null if SAS generation is not available.</returns>
    public Uri? GenerateSasUrl(
        string path,
        string? version = null,
        TimeSpan? expiresIn = null,
        BlobSasPermissions permissions = BlobSasPermissions.Read)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var blobClient = GetBlobClient(path, version);

        if (!blobClient.CanGenerateSasUri)
            return null;

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = NormalizePath(path),
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expiresIn ?? _options.DefaultSasExpiration)
        };

        sasBuilder.SetPermissions(permissions);

        if (version is not null)
        {
            sasBuilder.BlobVersionId = version;
        }

        return blobClient.GenerateSasUri(sasBuilder);
    }

    /// <summary>
    /// Merges new metadata with existing blob metadata.
    /// </summary>
    public async Task<IDictionary<string, string>> MergeMetadataAsync(
        string path,
        IDictionary<string, string> newMetadata,
        bool overwriteExisting = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(newMetadata);

        _logger?.LogInformation("Merging metadata for file {Path}, overwrite: {Overwrite}", path, overwriteExisting);

        var blobClient = _containerClient.GetBlobClient(NormalizePath(path));
        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        var mergedMetadata = new Dictionary<string, string>(properties.Value.Metadata);

        foreach (var (key, value) in newMetadata)
        {
            var prefixedKey = key.StartsWith(MetadataPrefix) ? key : $"{MetadataPrefix}{key}";

            if (overwriteExisting || !mergedMetadata.ContainsKey(prefixedKey))
            {
                mergedMetadata[prefixedKey] = value;
            }
        }

        await blobClient.SetMetadataAsync(mergedMetadata, cancellationToken: cancellationToken);

        var result = ExtractCustomMetadata(mergedMetadata);
        _logger?.LogInformation("Successfully merged metadata for file {Path}, resulting in {Count} custom metadata entries", path, result.Count);

        return result;
    }

    private BlobClient GetBlobClient(string path, string? version)
    {
        var normalizedPath = NormalizePath(path);
        var blobClient = _containerClient.GetBlobClient(normalizedPath);

        if (version is not null)
        {
            return blobClient.WithVersion(version);
        }

        return blobClient;
    }

    private static string NormalizePath(string path)
    {
        // Azure Blob Storage uses forward slashes
        return path.Replace('\\', '/').TrimStart('/');
    }

    private static Dictionary<string, string> BuildMetadata(
        IDictionary<string, string>? customMetadata,
        string? versionTag)
    {
        var metadata = new Dictionary<string, string>();

        if (customMetadata is not null)
        {
            foreach (var (key, value) in customMetadata)
            {
                // Prefix custom metadata to avoid conflicts
                var safeKey = SanitizeMetadataKey(key);
                metadata[$"{MetadataPrefix}{safeKey}"] = value;
            }
        }

        if (!string.IsNullOrWhiteSpace(versionTag))
        {
            metadata["version_tag"] = versionTag;
        }

        return metadata;
    }

    private static string SanitizeMetadataKey(string key)
    {
        // Azure metadata keys must be valid C# identifiers
        return new string(key.Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());
    }

    private static IDictionary<string, string> ExtractCustomMetadata(IDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return new Dictionary<string, string>();

        var customMetadata = new Dictionary<string, string>();

        foreach (var (key, value) in metadata)
        {
            if (key.StartsWith(MetadataPrefix))
            {
                customMetadata[key[MetadataPrefix.Length..]] = value;
            }
        }

        return customMetadata;
    }
}