namespace AIKit.Storage.Azure;

/// <summary>
/// Connection options for Azure Blob storage provider.
/// </summary>
public sealed class AzureBlobStorageConnectionOptions
{
    /// <summary>
    /// The Azure Storage connection string.
    /// </summary>
    public string ConnectionString { get; set; } = default!;

    /// <summary>
    /// The blob container name.
    /// </summary>
    public string ContainerName { get; set; } = default!;

    /// <summary>
    /// Additional Azure Blob storage options.
    /// </summary>
    public AzureBlobStorageOptions Options { get; set; } = new();
}