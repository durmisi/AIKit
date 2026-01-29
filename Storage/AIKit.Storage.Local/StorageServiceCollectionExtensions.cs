using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AIKit.Storage.Local;

/// <summary>
/// Extension methods for registering storage providers with dependency injection.
/// </summary>
public static class StorageServiceCollectionExtensions
{
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
        {
            var logger = sp.GetService<ILogger<LocalVersionedStorageProvider>>();
            return new StorageProviderBuilder()
            .WithBasePath(basePath)
            .WithLogger(logger)
            .Build();
        });

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

        services.TryAddSingleton<IStorageProvider>(sp =>
        {
            var logger = sp.GetService<ILogger<LocalVersionedStorageProvider>>();
            var builder = new StorageProviderBuilder();
            var options = new LocalStorageOptions();
            configure(options);
            return builder.WithOptions(options).WithLogger(logger).Build();
        });

        return services;
    }
}