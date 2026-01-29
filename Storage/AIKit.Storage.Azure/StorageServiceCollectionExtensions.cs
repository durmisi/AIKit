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

        var options = new AzureBlobStorageOptions();
        configure?.Invoke(options);

        services.TryAddSingleton<IStorageProvider>(sp =>
            new StorageProviderBuilder()
                .WithConnectionString(connectionString)
                .WithContainerName(containerName)
                .WithOptions(options)
                .Build());

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

        services.TryAddSingleton<IStorageProvider>(sp =>
            new StorageProviderBuilder()
                .WithConnectionString(connectionOptions.ConnectionString)
                .WithContainerName(connectionOptions.ContainerName)
                .WithOptions(connectionOptions.Options)
                .Build());

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
            new StorageProviderBuilder()
                .WithConnectionString(connectionString)
                .WithContainerName(containerName)
                .WithOptions(options)
                .Build());

        return services;
    }

    /// <summary>
    /// Adds the Azure Blob storage provider to the service collection using a builder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="builderFactory">Factory to create the storage provider builder.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureBlobStorage(
        this IServiceCollection services,
        Func<IServiceProvider, StorageProviderBuilder> builderFactory)
    {
        ArgumentNullException.ThrowIfNull(builderFactory);

        services.TryAddSingleton<IStorageProvider>(sp =>
        {
            var builder = builderFactory(sp);
            return builder.Build();
        });

        return services;
    }
}