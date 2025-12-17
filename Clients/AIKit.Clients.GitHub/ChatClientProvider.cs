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

    public string Provider => "github-models";

    public IChatClient Create()
        => Create(_defaultSettings);

    public IChatClient Create(AIClientSettings settings)
    {
        Validate(settings);

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(Constants.GitHubModelsEndpoint)
        };

        var credential = new ApiKeyCredential(settings.GitHubToken!);
        var client = new OpenAIClient(credential, options);

        return client.GetChatClient(settings.ModelId!).AsIChatClient();
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}
