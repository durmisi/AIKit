using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Core.Clients;

public sealed class EmbeddingProviderFactory
{
    private readonly ConcurrentDictionary<string, IEmbeddingProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

    public EmbeddingProviderFactory(IEnumerable<IEmbeddingProvider> providers)
    {
        foreach (var provider in providers)
        {
            _providers[provider.Provider] = provider;
        }
    }

    public void AddProvider(IEmbeddingProvider provider)
    {
        _providers[provider.Provider] = provider;
    }

    public IEmbeddingGenerator<string, Embedding<float>> Create(string provider, AIClientSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentNullException(nameof(provider));

        if (!_providers.TryGetValue(provider, out var embeddingProvider))
        {
            throw new InvalidOperationException(
                $"No IEmbeddingProvider registered for provider '{provider}'. " +
                $"Available providers: {string.Join(", ", _providers.Keys)}"
            );
        }

        if (settings is not null)
        {
            return embeddingProvider.Create(settings);
        }

        return embeddingProvider.Create();
    }
}