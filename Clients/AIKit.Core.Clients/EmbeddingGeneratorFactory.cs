using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Core.Clients;

/// <summary>
/// Factory for creating IEmbeddingGenerator instances from registered providers.
/// </summary>
/// <example>
/// Registering the factory and providers in DI:
/// <code>
/// // Register providers
/// services.AddSingleton&lt;IEmbeddingGeneratorProvider&gt;(new Gemini.EmbeddingGeneratorProvider(
///     new AIClientSettings { ApiKey = "key1", ModelId = "embedding-model", ProviderName = "gemini-embed" }));
/// 
/// // Register the factory
/// services.AddSingleton&lt;EmbeddingGeneratorFactory&gt;();
/// </code>
///
/// Usage:
/// <code>
/// var factory = serviceProvider.GetRequiredService&lt;EmbeddingGeneratorFactory&gt;();
/// var generator = factory.Create("gemini-embed");
/// var embeddings = await generator.GenerateAsync(new[] { "text" });
/// </code>
///
/// Dynamic registration from database:
/// <code>
/// public class EmbeddingProviderLoader
/// {
///     private readonly EmbeddingGeneratorFactory _factory;
///     private readonly MyDbContext _db;
/// 
///     public EmbeddingProviderLoader(EmbeddingGeneratorFactory factory, MyDbContext db)
///     {
///         _factory = factory;
///         _db = db;
///     }
/// 
///     public async Task LoadProvidersAsync()
///     {
///         var settings = await _db.AIClientSettings.ToListAsync();
///         foreach (var setting in settings)
///         {
///             var provider = new Gemini.EmbeddingGeneratorProvider(setting);
///             _factory.AddProvider(provider);
///         }
///     }
/// }
/// </code>
/// </example>
public sealed class EmbeddingGeneratorFactory
{
    private readonly ConcurrentDictionary<string, IEmbeddingGeneratorProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

    public EmbeddingGeneratorFactory(IEnumerable<IEmbeddingGeneratorProvider> providers)
    {
        foreach (var provider in providers)
        {
            _providers[provider.Provider] = provider;
        }
    }

    public void AddProvider(IEmbeddingGeneratorProvider provider)
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
                $"No IEmbeddingGeneratorProvider registered for provider '{provider}'. " +
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