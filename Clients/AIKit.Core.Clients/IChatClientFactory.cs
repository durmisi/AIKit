using Microsoft.Extensions.AI;

namespace AIKit.Core.Clients;

public interface IChatClientFactory
{
    IChatClient Create(string provider, string model);
}

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