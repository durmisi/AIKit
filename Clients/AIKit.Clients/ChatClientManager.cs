using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Clients;

public sealed class ChatClientManager
{
    private readonly ConcurrentDictionary<string, IChatClientFactory> _factories = new(StringComparer.OrdinalIgnoreCase);

    public ChatClientManager(IEnumerable<IChatClientFactory> factories)
    {
        foreach (var factory in factories)
        {
            _factories[factory.Provider] = factory;
        }
    }

    public void AddProvider(IChatClientFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
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