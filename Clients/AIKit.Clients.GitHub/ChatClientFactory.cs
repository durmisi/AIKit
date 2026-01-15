using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace AIKit.Clients.GitHub;

/// <summary>
/// Requires GitHubToken, ModelId. Uses https://models.github.ai/inference
/// </summary>
public sealed class ChatClientFactory : IChatClientFactory
{
    private readonly AIClientSettings _defaultSettings;
    private readonly ILogger<ChatClientFactory>? _logger;

    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;

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

        if (settings.HttpClient != null)
        {
            options.Transport = new HttpClientPipelineTransport(settings.HttpClient);
        }

        var credential = new ApiKeyCredential(settings.GitHubToken!);
        var client = new OpenAIClient(credential, options);

        var targetModel = model ?? settings.ModelId;
        _logger?.LogInformation("Creating GitHub Models chat client for model {Model}", targetModel);

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
