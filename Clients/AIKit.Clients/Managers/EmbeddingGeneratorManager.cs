using AIKit.Clients.Interfaces;
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Clients.Managers;

/// <summary>
/// Manages embedding generator factories for different AI providers, allowing creation of embedding generators by provider name.
/// </summary>
public sealed class EmbeddingGeneratorManager
{
    private readonly ConcurrentDictionary<string, IEmbeddingGeneratorFactory> _factories = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorManager"/> with the specified factories.
    /// </summary>
    /// <param name="factories">The collection of embedding generator factories to register.</param>
    public EmbeddingGeneratorManager(IEnumerable<IEmbeddingGeneratorFactory> factories)
    {
        foreach (var factory in factories)
        {
            _factories[factory.Provider] = factory;
        }
    }

    /// <summary>
    /// Adds a new provider factory to the manager.
    /// </summary>
    /// <param name="factory">The embedding generator factory to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if factory is null.</exception>
    public void AddProvider(IEmbeddingGeneratorFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factories[factory.Provider] = factory;
    }

    /// <summary>
    /// Creates an embedding generator for the specified provider with optional settings.
    /// </summary>
    /// <param name="provider">The name of the provider (e.g., "open-ai").</param>
    /// <param name="settings">Optional settings to use for the generator as key-value pairs. If null, uses factory defaults.</param>
    /// <returns>An <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> instance for the specified provider.</returns>
    /// <exception cref="ArgumentNullException">Thrown if provider is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no factory is registered for the provider.</exception>
    public IEmbeddingGenerator<string, Embedding<float>> Create(string provider, Dictionary<string, object>? settings = null)
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