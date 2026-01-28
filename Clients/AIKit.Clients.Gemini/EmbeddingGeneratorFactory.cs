using AIKit.Clients.Interfaces;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Gemini;

public sealed class EmbeddingGeneratorFactory
{
    private readonly Dictionary<string, object> _defaultSettings;

    public EmbeddingGeneratorFactory(Dictionary<string, object> settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "gemini";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(Dictionary<string, object> settings)
    {
        Validate(settings);

        var options = new GeminiClientOptions
        {
            ApiKey = (string)settings["ApiKey"],
            ModelId = (string)settings["ModelId"]
        };

        return new GeminiEmbeddingGenerator(options);
    }

    private static void Validate(Dictionary<string, object> settings)
    {
        if (!settings.TryGetValue("ApiKey", out var apiKey) || string.IsNullOrWhiteSpace(apiKey as string))
            throw new ArgumentException("ApiKey is required.", "ApiKey");

        if (!settings.TryGetValue("ModelId", out var modelId) || string.IsNullOrWhiteSpace(modelId as string))
            throw new ArgumentException("ModelId is required.", "ModelId");
    }
}