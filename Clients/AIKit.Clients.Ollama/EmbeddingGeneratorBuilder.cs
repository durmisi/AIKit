using AIKit.Clients.Ollama;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Ollama;

/// <summary>
/// Builder for creating Ollama embedding generators with maximum flexibility.
/// </summary>
public class EmbeddingGeneratorBuilder
{
    private string? _endpoint;
    private string? _modelId;
    private RetryPolicySettings? _retryPolicy;

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
    /// Sets the retry policy.
    /// </summary>
    /// <param name="retryPolicy">The retry policy settings.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithRetryPolicy(RetryPolicySettings retryPolicy)
    {
        _retryPolicy = retryPolicy;
        return this;
    }

    /// <summary>
    /// Builds the IEmbeddingGenerator instance.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Build()
    {
        var settings = new AIClientSettings
        {
            Endpoint = _endpoint,
            ModelId = _modelId,
            RetryPolicy = _retryPolicy
        };

        var factory = new EmbeddingGeneratorFactory(settings);
        return factory.Create();
    }
}