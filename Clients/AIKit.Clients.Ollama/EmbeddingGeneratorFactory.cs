using AIKit.Clients.Interfaces;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Ollama;

public sealed class EmbeddingGeneratorFactory
{
    private readonly Dictionary<string, object> _defaultSettings;

    public EmbeddingGeneratorFactory(Dictionary<string, object> settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "ollama";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(Dictionary<string, object> settings)
    {
        Validate(settings);

        var endpoint = (string)settings["Endpoint"];
        var uri = new Uri(endpoint);
        var modelId = (string)settings["ModelId"];
        return new OllamaEmbeddingGenerator(uri, modelId);
    }

    private static void Validate(Dictionary<string, object> settings)
    {
        if (!settings.TryGetValue("Endpoint", out var endpoint) || string.IsNullOrWhiteSpace(endpoint as string))
            throw new ArgumentException("Endpoint is required.", "Endpoint");

        if (!Uri.TryCreate(endpoint as string, UriKind.Absolute, out _))
            throw new ArgumentException("Endpoint must be a valid absolute URI.", "Endpoint");

        if (!settings.TryGetValue("ModelId", out var modelId) || string.IsNullOrWhiteSpace(modelId as string))
            throw new ArgumentException("ModelId is required.", "ModelId");
    }
}