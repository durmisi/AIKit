using AIKit.Clients.Base;
using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Azure.Core;
using Azure.Identity;
using elbruno.Extensions.AI.Claude;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.AzureClaude;

/// <summary>
/// Factory for creating Azure Claude chat clients.
/// Requires Endpoint, ModelId, and either ApiKey or UseDefaultAzureCredential.
/// </summary>
public sealed class ChatClientFactory : BaseChatClientFactory
{
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
        : base(settings, logger)
    {
    }

    protected override string GetDefaultProviderName() => "azure-claude";

    protected override void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);

        if (!settings.UseDefaultAzureCredential)
        {
            AIClientSettingsValidator.RequireApiKey(settings);
        }
    }

    protected override IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
        var endpoint = new Uri(settings.Endpoint!);

        var targetModelId = modelName ?? settings.ModelId;

        if (string.IsNullOrWhiteSpace(targetModelId))
        {
            throw new ArgumentException("ModelId must be provided either in settings or as modelName parameter.");
        }

        _logger?.LogInformation("Creating Azure Claude chat client for model {Model} at {Endpoint}", targetModelId, endpoint);

        if (settings.UseDefaultAzureCredential)
        {
            var credential = new DefaultAzureCredential();
            return new AzureClaudeClient(endpoint, targetModelId, credential, settings.HttpClient);
        }
        else
        {
            return new AzureClaudeClient(endpoint, targetModelId, settings.ApiKey!, settings.HttpClient);
        }
    }

    /// <summary>
    /// Creates a chat client with a custom TokenCredential.
    /// </summary>
    /// <param name="endpoint">The Azure endpoint URI.</param>
    /// <param name="modelId">The model ID to use.</param>
    /// <param name="credential">The TokenCredential for authentication.</param>
    /// <param name="httpClient">Optional HttpClient for requests.</param>
    /// <returns>The created chat client.</returns>
    public IChatClient Create(Uri endpoint, string modelId, TokenCredential credential, HttpClient? httpClient = null)
    {
        _logger?.LogInformation("Creating Azure Claude chat client with custom credential for model {Model} at {Endpoint}", modelId, endpoint);

        var client = new AzureClaudeClient(endpoint, modelId, credential, httpClient);

        if (_defaultSettings.RetryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", _defaultSettings.RetryPolicy.MaxRetries);
            return new RetryChatClient(client, _defaultSettings.RetryPolicy);
        }

        return client;
    }

    /// <summary>
    /// Creates a chat client with an API key.
    /// </summary>
    /// <param name="endpoint">The Azure endpoint URI.</param>
    /// <param name="modelId">The model ID to use.</param>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="httpClient">Optional HttpClient for requests.</param>
    /// <returns>The created chat client.</returns>
    public IChatClient Create(Uri endpoint, string modelId, string apiKey, HttpClient? httpClient = null)
    {
        _logger?.LogInformation("Creating Azure Claude chat client with API key for model {Model} at {Endpoint}", modelId, endpoint);

        var client = new AzureClaudeClient(endpoint, modelId, apiKey, httpClient);

        if (_defaultSettings.RetryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", _defaultSettings.RetryPolicy.MaxRetries);
            return new RetryChatClient(client, _defaultSettings.RetryPolicy);
        }

        return client;
    }
}