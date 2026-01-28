using AIKit.Clients.Interfaces;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Ollama;

/// <summary>
/// Builder for creating Ollama embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorBuilder 
{
    private string? _endpoint;
    private string? _modelId;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorBuilder"/>.
    /// </summary>
    public EmbeddingGeneratorBuilder()
    {
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => "ollama";

    /// <summary>
    /// Sets the endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint URL.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithEndpoint(string? endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        return this;
    }

    /// <summary>
    /// Sets the model ID.
    /// </summary>
    /// <param name="modelId">The model ID.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithModelId(string? modelId)
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
        if (string.IsNullOrWhiteSpace(_endpoint))
            throw new InvalidOperationException("Endpoint is required. Call WithEndpoint().");

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new InvalidOperationException("ModelId is required. Call WithModelId().");

        var uri = new Uri(_endpoint);
        return new OllamaEmbeddingGenerator(uri, _modelId);
    }
}