using Microsoft.Extensions.AI;

namespace AIKit.Clients;

/// <summary>
/// Provides chat clients for AI interactions.
/// Implementations create instances of <see cref="IChatClient"/> for specific AI providers.
/// </summary>
public interface IChatClientFactory
{
    /// <summary>
    /// Gets the name of the provider (e.g., "open-ai", "azure-open-ai").
    /// Used for identification and registration.
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Creates a chat client using the default settings.
    /// </summary>
    /// <param name="modelName">Optional model name to override the default.</param>
    /// <returns>An <see cref="IChatClient"/> instance.</returns>
    IChatClient Create(string? modelName = null);

    /// <summary>
    /// Creates a chat client with custom settings.
    /// </summary>
    /// <param name="settings">The settings to use for the client.</param>
    /// <param name="modelName">Optional model name to override the settings.</param>
    /// <returns>An <see cref="IChatClient"/> instance.</returns>
    IChatClient Create(AIClientSettings settings, string? modelName = null);
}