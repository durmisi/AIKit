using AIKit.Clients.Interfaces;
using AIKit.Clients.Settings;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Gemini;

public sealed class EmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    private readonly AIClientSettings _defaultSettings;

    public EmbeddingGeneratorFactory(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "gemini";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings)
    {
        Validate(settings);

        var options = new GeminiClientOptions
        {
            ApiKey = settings.ApiKey!,
            ModelId = settings.ModelId!
        };

        return new GeminiEmbeddingGenerator(options);
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}