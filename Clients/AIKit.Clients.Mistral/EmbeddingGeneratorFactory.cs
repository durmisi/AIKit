using AIKit.Clients.Interfaces;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.Mistral;

public sealed class EmbeddingGeneratorFactory
{
    private readonly Dictionary<string, object> _defaultSettings;

    public EmbeddingGeneratorFactory(Dictionary<string, object> settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "mistral";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(Dictionary<string, object> settings)
    {
        Validate(settings);

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://api.mistral.ai/v1/")
        };

        var credential = new ApiKeyCredential((string)settings["ApiKey"]);
        var client = new OpenAIClient(credential, options);

        var targetModel = (string)settings["ModelId"];

        return client.GetEmbeddingClient(targetModel).AsIEmbeddingGenerator();
    }

    private static void Validate(Dictionary<string, object> settings)
    {
        if (!settings.TryGetValue("ApiKey", out var apiKey) || string.IsNullOrWhiteSpace(apiKey as string))
            throw new ArgumentException("ApiKey is required.", "ApiKey");

        if (!settings.TryGetValue("ModelId", out var modelId) || string.IsNullOrWhiteSpace(modelId as string))
            throw new ArgumentException("ModelId is required.", "ModelId");
    }
}