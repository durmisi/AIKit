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
    protected readonly AIClientSettings _defaultSettings;
    protected readonly ILogger? _logger;

    protected BaseChatClientFactory(AIClientSettings settings, ILogger? logger = null)
    {
        _defaultSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;
        Validate(_defaultSettings);
    }

    public string Provider => _defaultSettings.ProviderName ?? GetDefaultProviderName();

    public IChatClient Create(string? modelName = null)
        => Create(_defaultSettings, modelName);

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