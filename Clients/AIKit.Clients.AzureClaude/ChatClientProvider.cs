using AIKit.Core.Clients;
using Azure.Identity;
using elbruno.Extensions.AI.Claude;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.AzureClaude;

/// <summary>
/// Requires Endpoint, ModelId, and either ApiKey or UseDefaultAzureCredential.
/// </summary>
public sealed class ChatClientProvider : IChatClientProvider
{
    private readonly AIClientSettings _defaultSettings;

    public ChatClientProvider(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "azure-claude";

    public IChatClient Create()
        => Create(_defaultSettings);

    public IChatClient Create(AIClientSettings settings)
    {
        Validate(settings);

        var endpoint = new Uri(settings.Endpoint!);

        if (settings.UseDefaultAzureCredential)
        {
            var credential = new DefaultAzureCredential();
            return new AzureClaudeClient(endpoint, settings.ModelId!, credential);
        }
        else
        {
            return new AzureClaudeClient(endpoint, settings.ModelId!, settings.ApiKey!);
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
