using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Core.Clients;

public interface IChatClientFactory
{
    IChatClient Create(string provider, string model);
}

public sealed class ChatClientFactory : IChatClientFactory
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