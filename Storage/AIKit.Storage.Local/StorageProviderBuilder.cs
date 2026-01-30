using Microsoft.Extensions.Logging;

namespace AIKit.Storage.Local;

/// <summary>
/// Builder for creating local storage providers with maximum flexibility.
/// </summary>
public sealed class StorageProviderBuilder
{
    private string? _basePath;
    private LocalStorageOptions? _options;
    private ILogger<LocalVersionedStorageProvider>? _logger;

    /// <summary>
    /// Sets the base path for storing files locally.
    /// </summary>
    /// <param name="basePath">The base path.</param>
    /// <returns>The builder instance.</returns>
    public StorageProviderBuilder WithBasePath(string basePath)
    {
        _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        return this;
    }

    /// <summary>
    /// Sets the local storage options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <returns>The builder instance.</returns>
    public StorageProviderBuilder WithOptions(LocalStorageOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Sets the logger for the storage provider.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <returns>The builder instance.</returns>
    public StorageProviderBuilder WithLogger(ILogger<LocalVersionedStorageProvider>? logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    /// Builds the local storage provider.
    /// </summary>
    /// <returns>The configured storage provider.</returns>
    /// <exception cref="InvalidOperationException">Thrown if required fields are not configured.</exception>
    public IStorageProvider Build()
    {
        string basePath;

        if (!string.IsNullOrWhiteSpace(_basePath))
        {
            basePath = _basePath;
        }
        else if (_options != null && !string.IsNullOrWhiteSpace(_options.BasePath))
        {
            basePath = _options.BasePath;
        }
        else
        {
            throw new InvalidOperationException("BasePath must be configured for local storage.");
        }

        return new LocalVersionedStorageProvider(basePath, _logger);
    }
}