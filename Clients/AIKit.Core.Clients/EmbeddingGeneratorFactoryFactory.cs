using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Core.Clients;

public sealed class EmbeddingGeneratorFactoryFactory
{
    private readonly ConcurrentDictionary<string, IEmbeddingGeneratorFactory> _providers = new(StringComparer.OrdinalIgnoreCase);

    public EmbeddingGeneratorFactoryFactory(IEnumerable<IEmbeddingGeneratorFactory> providers)
    {
        foreach (var provider in providers)
        {
            _providers[provider.Provider] = provider;
        }
    }

    public void AddProvider(IEmbeddingGeneratorFactory provider)
    {
        _providers[provider.Provider] = provider;
    }

    public IEmbeddingGenerator<string, Embedding<float>> Create(string provider, AIClientSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentNullException(nameof(provider));

        if (!_providers.TryGetValue(provider, out var EmbeddingGeneratorFactory))
        {
            throw new InvalidOperationException(
                $"No IEmbeddingGeneratorFactory registered for provider '{provider}'. " +
                $"Available providers: {string.Join(", ", _providers.Keys)}"
            );
        }

        if (settings is not null)
        {
            return EmbeddingGeneratorFactory.Create(settings);
        }

        return EmbeddingGeneratorFactory.Create();
    }
}