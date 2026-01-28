using AIKit.Clients.Interfaces;
using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Base;

/// <summary>
/// Base class for chat client factories, providing common functionality.
/// </summary>
public abstract class BaseChatClientFactory : IChatClientFactory
{
    /// <summary>
    /// The default AI client settings.
    /// </summary>
    protected readonly AIClientSettings _defaultSettings;
    /// <summary>
    /// The optional logger instance.
    /// </summary>
    protected readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseChatClientFactory"/> class.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <param name="logger">Optional logger instance.</param>
    protected BaseChatClientFactory(AIClientSettings settings, ILogger? logger = null)
    {
        _defaultSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;
        Validate(_defaultSettings);
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => _defaultSettings.ProviderName ?? GetDefaultProviderName();

    /// <summary>
    /// Creates a chat client using the default settings.
    /// </summary>
    /// <param name="modelName">Optional model name to use for the client.</param>
    /// <returns>The created chat client.</returns>
    public IChatClient Create(string? modelName = null)
        => Create(_defaultSettings, modelName);

    /// <summary>
    /// Creates a chat client with the specified settings.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <param name="modelName">Optional model name to use for the client.</param>
    /// <returns>The created chat client.</returns>
    public IChatClient Create(AIClientSettings settings, string? modelName = null)
    {
        Validate(settings);

        var client = CreateClient(settings, modelName);

        if (settings.RetryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", settings.RetryPolicy.MaxRetries);
            return new RetryChatClient(client, settings.RetryPolicy);
        }

        return client;
    }

    /// <summary>
    /// Gets the default provider name if not specified in settings.
    /// </summary>
    protected abstract string GetDefaultProviderName();

    /// <summary>
    /// Validates the settings for this provider.
    /// </summary>
    protected abstract void Validate(AIClientSettings settings);

    /// <summary>
    /// Creates the actual chat client instance.
    /// </summary>
    protected abstract IChatClient CreateClient(AIClientSettings settings, string? modelName);
}