using AIKit.Clients.Ollama;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Ollama;

/// <summary>
/// Builder for creating Ollama embedding generators with maximum flexibility.
/// </summary>
public class EmbeddingGeneratorBuilder
{
    private string? _endpoint;
    private string? _modelId;

    /// <summary>
    /// Sets the Ollama endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint URL.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithEndpoint(string endpoint)
    {
        _endpoint = endpoint;
        return this;
    }

    /// <summary>
    /// Sets the model ID.
    /// </summary>
    /// <param name="modelId">The model identifier.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithModel(string modelId)
    {
        _modelId = modelId;
        return this;
    }

    /// <summary>
    /// Builds the IEmbeddingGenerator instance.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Build()
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
            throw new ArgumentException("Endpoint is required.", nameof(_endpoint));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));

        var settings = new Dictionary<string, object>
        {
            ["Endpoint"] = _endpoint,
            ["ModelId"] = _modelId
        };

        var factory = new EmbeddingGeneratorFactory(settings);
        return factory.Create();
    }
}