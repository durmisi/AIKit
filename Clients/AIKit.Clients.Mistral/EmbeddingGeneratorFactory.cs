using AIKit.Core.Clients;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.Mistral;

public sealed class EmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    private readonly AIClientSettings _defaultSettings;

    public EmbeddingGeneratorFactory(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => _defaultSettings.ProviderName ?? "mistral";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings)
    {
        Validate(settings);

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://api.mistral.ai/v1/")
        };

        var credential = new ApiKeyCredential(settings.ApiKey!);
        var client = new OpenAIClient(credential, options);

        var targetModel = settings.ModelId!;

        return client.GetEmbeddingClient(targetModel).AsIEmbeddingGenerator();
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}