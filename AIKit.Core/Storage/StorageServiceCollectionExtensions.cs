using Amazon.S3;
using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIKit.Core.Storage;

/// <summary>
/// Extension methods for registering storage providers with dependency injection.
/// </summary>
public static class StorageServiceCollectionExtensions
{
    #region Local Storage

    /// <summary>
    /// Adds the local versioned storage provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="basePath">The base path for storing files.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLocalVersionedStorage(
        this IServiceCollection services,
        string basePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);

        services.TryAddSingleton<IStorageProvider>(sp =>
            new LocalVersionedStorageProvider(basePath));

        return services;
    }

    /// <summary>
    /// Adds the local versioned storage provider to the service collection with configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure local storage options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLocalVersionedStorage(
        this IServiceCollection services,
        Action<LocalStorageOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new LocalStorageOptions();
        configure(options);

        if (string.IsNullOrWhiteSpace(options.BasePath))
        {
            throw new InvalidOperationException("BasePath must be configured for local storage.");
        }

        services.TryAddSingleton<IStorageProvider>(sp =>
            new LocalVersionedStorageProvider(options.BasePath));

        return services;
    }

    #endregion

    #region Azure Blob Storage

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

        var options = new AzureBlobStorageOptions();
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

    #endregion

    #region Keyed Services

    /// <summary>
    /// Adds a keyed local versioned storage provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceKey">The service key for resolving the provider.</param>
    /// <param name="basePath">The base path for storing files.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddKeyedLocalVersionedStorage(
        this IServiceCollection services,
        string serviceKey,
        string basePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(basePath);

        services.AddKeyedSingleton<IStorageProvider>(serviceKey, (sp, key) =>
            new LocalVersionedStorageProvider(basePath));

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

    #endregion
}

#region Configuration Options

/// <summary>
/// Configuration options for local storage provider.
/// </summary>
public sealed class LocalStorageOptions
{
    /// <summary>
    /// The base path for storing files locally.
    /// </summary>
    public string BasePath { get; set; } = default!;
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

#endregion
