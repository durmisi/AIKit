using AIKit.Core.Clients;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.GitHub;

/// <summary>
/// Requires GitHubToken, ModelId. Uses https://models.github.ai/inference
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

    public string Provider => _defaultSettings.ProviderName ?? "github-models";

    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(Constants.GitHubModelsEndpoint)
        };

        var credential = new ApiKeyCredential(settings.GitHubToken!);
        var client = new OpenAIClient(credential, options);

        var targetModel = model ?? settings.ModelId;    

        return client.GetChatClient(targetModel).AsIChatClient();
    }

    private static void Validate(AIClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.GitHubToken))
            throw new ArgumentException(
                "GitHubToken is required.",
                nameof(AIClientSettings.GitHubToken));

        AIClientSettingsValidator.RequireModel(settings);
    }
}
