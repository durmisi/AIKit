using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace AIKit.Clients;

/// <summary>
/// Manages chat client factories for different AI providers, allowing creation of chat clients by provider name.
/// </summary>
public sealed class ChatClientManager
{
    private readonly ConcurrentDictionary<string, IChatClientFactory> _factories = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatClientManager"/> with the specified factories.
    /// </summary>
    /// <param name="factories">The collection of chat client factories to register.</param>
    public ChatClientManager(IEnumerable<IChatClientFactory> factories)
    {
        foreach (var factory in factories)
        {
            _factories[factory.Provider] = factory;
        }
    }

    /// <summary>
    /// Adds a new provider factory to the manager.
    /// </summary>
    /// <param name="factory">The chat client factory to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if factory is null.</exception>
    public void AddProvider(IChatClientFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factories[factory.Provider] = factory;
    }

    /// <summary>
    /// Creates a chat client for the specified provider and model.
    /// </summary>
    /// <param name="provider">The name of the provider (e.g., "open-ai").</param>
    /// <param name="model">The model name to use. If null or empty, uses the factory's default.</param>
    /// <returns>An <see cref="IChatClient"/> instance for the specified provider and model.</returns>
    /// <exception cref="ArgumentNullException">Thrown if provider or model is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no factory is registered for the provider.</exception>
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