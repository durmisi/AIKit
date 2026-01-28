using AIKit.Clients.Base;
using AIKit.Clients.Settings;
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
            return new AzureClaudeClient(endpoint, targetModelId, credential);
        }
        else
        {
            return new AzureClaudeClient(endpoint, targetModelId, settings.ApiKey!);
        }
    }
}