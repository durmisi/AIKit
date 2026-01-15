using Microsoft.Extensions.AI;

namespace AIKit.Clients.Ollama;

public sealed class EmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    private readonly AIClientSettings _defaultSettings;

    public EmbeddingGeneratorFactory(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "ollama";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings)
    {
        Validate(settings);

        var endpoint = new Uri(settings.Endpoint!);
        return new OllamaEmbeddingGenerator(endpoint, settings.ModelId!);
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}