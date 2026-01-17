using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIKit.Storage.Azure;

/// <summary>
/// Extension methods for registering storage providers with dependency injection.
/// </summary>
public static class StorageServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Azure Blob storage provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The Azure Storage connection string.</param>
    /// <param name="containerName">The blob container name.</param>
    /// <param name="configure">Optional action to configure Azure Blob storage options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureBlobStorage(
        this IServiceCollection services,
        string connectionString,
        string containerName,
        Action<AzureBlobStorageOptions>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

        var options = new AzureBlobStorageOptions() { };
        configure?.Invoke(options);

        services.TryAddSingleton<IStorageProvider>(sp =>
            new AzureBlobStorageProvider(connectionString, containerName, options));

        return services;
    }

    /// <summary>
    /// Adds the Azure Blob storage provider to the service collection using a BlobContainerClient.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="containerClientFactory">Factory to create the BlobContainerClient.</param>
    /// <param name="configure">Optional action to configure Azure Blob storage options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureBlobStorage(
        this IServiceCollection services,
        Func<IServiceProvider, BlobContainerClient> containerClientFactory,
        Action<AzureBlobStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(containerClientFactory);

        var options = new AzureBlobStorageOptions();
        configure?.Invoke(options);

        services.TryAddSingleton<IStorageProvider>(sp =>
        {
            var containerClient = containerClientFactory(sp);
            return new AzureBlobStorageProvider(containerClient, options);
        });

        return services;
    }

    /// <summary>
    /// Adds the Azure Blob storage provider to the service collection with full configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure Azure Blob storage connection options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureBlobStorage(
        this IServiceCollection services,
        Action<AzureBlobStorageConnectionOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var connectionOptions = new AzureBlobStorageConnectionOptions();
        configure(connectionOptions);

        if (string.IsNullOrWhiteSpace(connectionOptions.ConnectionString))
        {
            throw new InvalidOperationException("ConnectionString must be configured for Azure Blob storage.");
        }

        if (string.IsNullOrWhiteSpace(connectionOptions.ContainerName))
        {
            throw new InvalidOperationException("ContainerName must be configured for Azure Blob storage.");
        }

        services.TryAddSingleton<IStorageProvider>(sp =>
            new AzureBlobStorageProvider(
                connectionOptions.ConnectionString,
                connectionOptions.ContainerName,
                connectionOptions.Options));

        return services;
    }

    /// <summary>
    /// Adds a keyed Azure Blob storage provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceKey">The service key for resolving the provider.</param>
    /// <param name="connectionString">The Azure Storage connection string.</param>
    /// <param name="containerName">The blob container name.</param>
    /// <param name="configure">Optional action to configure Azure Blob storage options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKeyedAzureBlobStorage(
        this IServiceCollection services,
        string serviceKey,
        string connectionString,
        string containerName,
        Action<AzureBlobStorageOptions>? configure = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);

        var options = new AzureBlobStorageOptions();
        configure?.Invoke(options);

        services.AddKeyedSingleton<IStorageProvider>(serviceKey, (sp, key) =>
            new AzureBlobStorageProvider(connectionString, containerName, options));

        return services;
    }
}

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