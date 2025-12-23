using Microsoft.Extensions.VectorData;

namespace AIKit.Core.VectorStores;

public sealed class VectorStoreResolver
{
    private readonly Dictionary<string, IVectorStoreFactory> _factories;

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

    public void Register(IVectorStoreFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (string.IsNullOrWhiteSpace(factory.Provider))
            throw new ArgumentException("Factory.Provider must be set");

        _factories[factory.Provider] = factory;
    }

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
