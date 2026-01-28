using AIKit.Clients.AzureOpenAI;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.AzureOpenAI;

/// <summary>
/// Builder for creating Azure OpenAI embedding generators with maximum flexibility.
/// </summary>
public class EmbeddingGeneratorBuilder
{
    private string? _endpoint;
    private string? _modelId;
    private string? _apiKey;
    private bool _useDefaultAzureCredential;
    private RetryPolicySettings? _retryPolicy;

    /// <summary>
    /// Sets the Azure OpenAI endpoint.
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
    /// Sets the API key for authentication.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey;
        _useDefaultAzureCredential = false;
        return this;
    }

    /// <summary>
    /// Configures to use default Azure credential for authentication.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithDefaultAzureCredential()
    {
        _useDefaultAzureCredential = true;
        _apiKey = null;
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
            ApiKey = _apiKey,
            UseDefaultAzureCredential = _useDefaultAzureCredential,
            RetryPolicy = _retryPolicy
        };

        var factory = new EmbeddingGeneratorFactory(settings);
        return factory.Create();
    }
}