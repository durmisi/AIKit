using AIKit.Clients.Interfaces;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.GitHub;

public sealed class EmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    private readonly Dictionary<string, object> _defaultSettings;

    public EmbeddingGeneratorFactory(Dictionary<string, object> settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "github-models";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(Dictionary<string, object> settings)
    {
        Validate(settings);

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(Constants.GitHubModelsEndpoint)
        };

        var gitHubToken = (string)settings["GitHubToken"];
        var credential = new ApiKeyCredential(gitHubToken);
        var client = new OpenAIClient(credential, options);
        var modelId = (string)settings["ModelId"];
        return client.GetEmbeddingClient(modelId).AsIEmbeddingGenerator();
    }

    private static void Validate(Dictionary<string, object> settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (!settings.TryGetValue("GitHubToken", out var gitHubToken) || string.IsNullOrWhiteSpace(gitHubToken as string))
            throw new ArgumentException("GitHubToken is required.", "GitHubToken");

        if (!settings.TryGetValue("ModelId", out var modelId) || string.IsNullOrWhiteSpace(modelId as string))
            throw new ArgumentException("ModelId is required.", "ModelId");
    }
}