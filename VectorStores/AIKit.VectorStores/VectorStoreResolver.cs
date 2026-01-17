using Microsoft.Extensions.VectorData;

namespace AIKit.VectorStores;

/// <summary>
/// Resolves vector store instances by provider name.
/// </summary>
public sealed class VectorStoreResolver
{
    private readonly Dictionary<string, IVectorStoreFactory> _factories;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorStoreResolver"/> class.
    /// </summary>
    /// <param name="diFactories">The collection of vector store factories from dependency injection.</param>
    public VectorStoreResolver(
        IEnumerable<IVectorStoreFactory> diFactories)
    {
        _factories = new Dictionary<string, IVectorStoreFactory>(
            StringComparer.OrdinalIgnoreCase);

        // Register DI factories first
        foreach (var factory in diFactories)
        {
            Register(factory);
        }
    }

    /// <summary>
    /// Registers a vector store factory.
    /// </summary>
    /// <param name="factory">The factory to register.</param>
    /// <exception cref="ArgumentNullException">Thrown if factory is null.</exception>
    /// <exception cref="ArgumentException">Thrown if factory.Provider is null or whitespace.</exception>
    public void Register(IVectorStoreFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (string.IsNullOrWhiteSpace(factory.Provider))
            throw new ArgumentException("Factory.Provider must be set");

        _factories[factory.Provider] = factory;
    }

    /// <summary>
    /// Resolves a vector store instance for the specified provider.
    /// </summary>
    /// <param name="provider">The name of the provider.</param>
    /// <returns>A <see cref="VectorStore"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no factory is registered for the provider.</exception>
    public VectorStore Resolve(string provider)
    {
        if (!_factories.TryGetValue(provider, out var factory))
        {
            throw new InvalidOperationException(
                $"No vector store factory registered for provider '{provider}'.");
        }

        return factory.Create();
    }
}