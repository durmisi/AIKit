using Microsoft.Extensions.AI;

namespace AIKit.Core.Clients;

public interface IChatClientFactory
{
    IChatClient Create(string provider, string model);
}

/// <summary>
/// Factory for creating IChatClient instances from registered providers.
/// </summary>
/// <example>
/// Registering multiple providers with different settings:
/// <code>
/// // Register OpenAI for GPT-4
/// services.AddSingleton&lt;IChatClientProvider&gt;(new OpenAI.ChatClientProvider(
///     new AIClientSettings { ApiKey = "key1", ModelId = "gpt-4", ProviderName = "open-ai-gpt4" }));
/// 
/// // Register OpenAI for GPT-3.5
/// services.AddSingleton&lt;IChatClientProvider&gt;(new OpenAI.ChatClientProvider(
///     new AIClientSettings { ApiKey = "key2", ModelId = "gpt-3.5-turbo", ProviderName = "open-ai-gpt35" }));
/// 
/// // Usage
/// var client = factory.Create("open-ai-gpt4", "gpt-4");
/// </code>
/// </example>
public sealed class ChatClientFactory : IChatClientFactory
{
    private readonly IReadOnlyDictionary<string, IChatClientProvider> _providers;

    public ChatClientFactory(IEnumerable<IChatClientProvider> providers)
    {
        _providers = providers.ToDictionary(
            p => p.Provider,
            p => p,
            StringComparer.OrdinalIgnoreCase
        );
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