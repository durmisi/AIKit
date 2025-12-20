using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Core.Clients;

/// <summary>
/// Factory for creating IChatClient instances from registered providers.
/// </summary>
/// <example>
/// Registering the factory and providers in DI:
/// <code>
/// // Register providers
/// services.AddSingleton<IChatClientProvider>(new OpenAI.ChatClientProvider(
///     new AIClientSettings { ApiKey = "key1", ModelId = "gpt-4", ProviderName = "open-ai-gpt4" }));
/// services.AddSingleton<IChatClientProvider>(new OpenAI.ChatClientProvider(
///     new AIClientSettings { ApiKey = "key2", ModelId = "gpt-3.5-turbo", ProviderName = "open-ai-gpt35" }));
/// 
/// // Register the factory
/// services.AddSingleton&lt;ChatClientFactory&gt;();
/// </code>
///
/// Usage:
/// <code>
/// var factory = serviceProvider.GetRequiredService&lt;ChatClientFactory&gt;();
/// var client = factory.Create("open-ai-gpt4", "gpt-4");
/// </code>
///
/// Dynamic registration from database:
/// <code>
/// public class ProviderLoader
/// {
///     private readonly ChatClientFactory _factory;
///     private readonly MyDbContext _db;
/// 
///     public ProviderLoader(ChatClientFactory factory, MyDbContext db)
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
///             var provider = new OpenAI.ChatClientProvider(setting);
///             _factory.AddProvider(provider);
///         }
///     }
/// }
/// </code>
/// </example>
public sealed class ChatClientFactory
{
    private readonly ConcurrentDictionary<string, IChatClientProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

    public ChatClientFactory(IEnumerable<IChatClientProvider> providers)
    {
        foreach (var provider in providers)
        {
            _providers[provider.Provider] = provider;
        }
    }

    public void AddProvider(IChatClientProvider provider)
    {
        _providers[provider.Provider] = provider;
    }

    public IChatClient Create(string provider, string? model = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentNullException(nameof(provider));

        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentNullException(nameof(model));

        if (!_providers.TryGetValue(provider, out var chatProvider))
        {
            throw new InvalidOperationException(
                $"No IChatClientProvider registered for provider '{provider}'. " +
                $"Available providers: {string.Join(", ", _providers.Keys)}"
            );
        }

        if (!string.IsNullOrEmpty(model))
        {
            return chatProvider.Create(model);
        }

        return chatProvider.Create();
    }
}