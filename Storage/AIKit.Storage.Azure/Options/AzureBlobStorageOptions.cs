namespace AIKit.Storage.Azure;

/// <summary>
/// Configuration options for Azure Blob Storage provider.
/// </summary>
public sealed class AzureBlobStorageOptions
{
    /// <summary>
    /// Default expiration time for generated SAS URLs.
    /// </summary>
    public TimeSpan DefaultSasExpiration { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Whether to automatically create the container if it doesn't exist.
    /// </summary>
    public bool AutoCreateContainer { get; set; } = true;
}