using Microsoft.Extensions.AI;

namespace AIKit.Clients.Gemini;

/// <summary>
/// Builder for creating Gemini embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorBuilder
{
    private string? _apiKey;
    private string? _modelId;

    /// <summary>
    /// Sets the API key.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        return this;
    }

    /// <summary>
    /// Sets the model ID.
    /// </summary>
    /// <param name="modelId">The model ID.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithModelId(string modelId)
    {
        _modelId = modelId ?? throw new ArgumentNullException(nameof(modelId));
        return this;
    }

    /// <summary>
    /// Builds the IEmbeddingGenerator instance.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Build()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("ApiKey is required. Call WithApiKey().");

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new InvalidOperationException("ModelId is required. Call WithModelId().");

        return ClientCreator.CreateGeminiEmbeddingGenerator(_apiKey, _modelId);
    }
}