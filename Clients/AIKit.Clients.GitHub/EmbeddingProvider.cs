using AIKit.Core.Clients;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.GitHub;

public sealed class EmbeddingProvider : IEmbeddingProvider
{
    private readonly AIClientSettings _defaultSettings;

    public EmbeddingProvider(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "github-models";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings)
    {
        Validate(settings);
        
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(Constants.GitHubModelsEndpoint)
        };

        var credential = new ApiKeyCredential(settings.GitHubToken!);
        var client = new OpenAIClient(credential, options);
        return client.GetEmbeddingClient(settings.ModelId!).AsIEmbeddingGenerator();

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
