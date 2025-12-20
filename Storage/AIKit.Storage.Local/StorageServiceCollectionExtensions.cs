using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AIKit.Core.Storage;

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

}


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

