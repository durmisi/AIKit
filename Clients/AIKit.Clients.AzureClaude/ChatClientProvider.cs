using AIKit.Core.Clients;
using Azure.Identity;
using elbruno.Extensions.AI.Claude;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.AzureClaude;

/// <summary>
/// Requires Endpoint, ModelId, and either ApiKey or UseDefaultAzureCredential.
/// </summary>
public sealed class ChatClientProvider : IChatClientProvider
{
    private readonly AIClientSettings _defaultSettings;
    private readonly ILogger<ChatClientProvider>? _logger;

    public ChatClientProvider(AIClientSettings settings, ILogger<ChatClientProvider>? logger = null)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;

        Validate(_defaultSettings);
    }

    public string Provider => _defaultSettings.ProviderName ?? "azure-claude";

    public IChatClient Create(string? modelName = null)
        => Create(_defaultSettings);

    public IChatClient Create(AIClientSettings settings, string? modelName = null)
    {
        Validate(settings);

        var endpoint = new Uri(settings.Endpoint!);

        var targetModelId = modelName ?? settings.ModelId;  

        if(string.IsNullOrWhiteSpace(targetModelId))
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

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);

        if (!settings.UseDefaultAzureCredential)
        {
            AIClientSettingsValidator.RequireApiKey(settings);
        }
    }
}
