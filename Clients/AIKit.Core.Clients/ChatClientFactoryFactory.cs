using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Core.Clients;

public sealed class ChatClientFactoryFactory
{
    private readonly ConcurrentDictionary<string, IChatClientFactory> _factories = new(StringComparer.OrdinalIgnoreCase);

    public ChatClientFactoryFactory(IEnumerable<IChatClientFactory> factories)
    {
        foreach (var factory in factories)
        {
            _factories[factory.Provider] = factory;
        }
    }

    public void AddProvider(IChatClientFactory factory)
    {
        _factories[factory.Provider] = factory;
    }

    public IChatClient Create(string provider, string? model = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentNullException(nameof(provider));

        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentNullException(nameof(model));

        if (!_factories.TryGetValue(provider, out var factory))
        {
            throw new InvalidOperationException(
                $"No IChatClientFactory registered for provider '{provider}'. " +
                $"Available providers: {string.Join(", ", _factories.Keys)}"
            );
        }

        if (!string.IsNullOrEmpty(model))
        {
            return factory.Create(model);
        }

        return factory.Create();
    }
}