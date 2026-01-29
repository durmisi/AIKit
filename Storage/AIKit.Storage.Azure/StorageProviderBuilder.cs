namespace AIKit.Storage.Azure;

/// <summary>
/// Builder for creating Azure Blob Storage providers with maximum flexibility.
/// </summary>
public sealed class StorageProviderBuilder
{
    private string? _connectionString;
    private string? _containerName;
    private AzureBlobStorageOptions? _options;

    /// <summary>
    /// Sets the Azure Storage connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>The builder instance.</returns>
    public StorageProviderBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        return this;
    }

    /// <summary>
    /// Sets the blob container name.
    /// </summary>
    /// <param name="containerName">The container name.</param>
    /// <returns>The builder instance.</returns>
    public StorageProviderBuilder WithContainerName(string containerName)
    {
        _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
        return this;
    }

    /// <summary>
    /// Sets the Azure Blob storage options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>The builder instance.</returns>
    public StorageProviderBuilder WithOptions(AzureBlobStorageOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Builds the Azure Blob Storage provider.
    /// </summary>
    /// <returns>The configured storage provider.</returns>
    /// <exception cref="InvalidOperationException">Thrown if required fields are not configured.</exception>
    public IStorageProvider Build()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("ConnectionString must be configured for Azure Blob storage.");
        }

        if (string.IsNullOrWhiteSpace(_containerName))
        {
            throw new InvalidOperationException("ContainerName must be configured for Azure Blob storage.");
        }

        var options = _options ?? new AzureBlobStorageOptions();
        return new AzureBlobStorageProvider(_connectionString, _containerName, options);
    }
}