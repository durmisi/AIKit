using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Core.Clients;

public sealed class EmbeddingGeneratorFactoryFactory
{
    private readonly ConcurrentDictionary<string, IEmbeddingGeneratorFactory> _factories = new(StringComparer.OrdinalIgnoreCase);

    public EmbeddingGeneratorFactoryFactory(IEnumerable<IEmbeddingGeneratorFactory> factories)
    {
        foreach (var factory in factories)
        {
            _factories[factory.Provider] = factory;
        }
    }

    public void AddProvider(IEmbeddingGeneratorFactory factory)
    {
        _factories[factory.Provider] = factory;
    }

    public IEmbeddingGenerator<string, Embedding<float>> Create(string provider, AIClientSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentNullException(nameof(provider));

        if (!_factories.TryGetValue(provider, out var EmbeddingGeneratorFactory))
        {
            throw new InvalidOperationException(
                $"No IEmbeddingGeneratorFactory registered for provider '{provider}'. " +
                $"Available providers: {string.Join(", ", _factories.Keys)}"
            );
        }

        if (settings is not null)
        {
            return EmbeddingGeneratorFactory.Create(settings);
        }

        return EmbeddingGeneratorFactory.Create();
    }
}