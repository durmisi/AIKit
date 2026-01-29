using AIKit.Clients.Resilience;
using Azure;
using Azure.AI.Inference;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.AzureOpenAI;

/// <summary>
/// Builder for creating Azure OpenAI embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorBuilder
{
    private string? _endpoint;
    private string? _modelId;
    private string? _apiKey;
    private bool _useDefaultAzureCredential;
    private TokenCredential? _tokenCredential;
    private RetryPolicySettings? _retryPolicy;

    /// <summary>
    /// Sets the Azure OpenAI endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint URL.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithEndpoint(string endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
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
    /// Sets the API key.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _useDefaultAzureCredential = false;
        _tokenCredential = null;
        return this;
    }

    /// <summary>
    /// Configures to use the default Azure credential.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithDefaultAzureCredential()
    {
        _useDefaultAzureCredential = true;
        _apiKey = null;
        _tokenCredential = null;
        return this;
    }

    /// <summary>
    /// Configures to use a custom Azure token credential for authentication.
    /// </summary>
    /// <param name="credential">The token credential.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithTokenCredential(TokenCredential credential)
    {
        _tokenCredential = credential ?? throw new ArgumentNullException(nameof(credential));
        _useDefaultAzureCredential = false;
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
        _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
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

        if (_tokenCredential == null && !_useDefaultAzureCredential && string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("Either ApiKey, DefaultAzureCredential, or TokenCredential is required. Call WithApiKey(), WithDefaultAzureCredential(), or WithTokenCredential().");

        EmbeddingsClient client;

        if (_tokenCredential != null)
        {
            client = new EmbeddingsClient(
                new Uri(_endpoint!),
                _tokenCredential);
        }
        else if (_useDefaultAzureCredential)
        {
            client = new EmbeddingsClient(
                new Uri(_endpoint!),
                new DefaultAzureCredential());
        }
        else
        {
            client = new EmbeddingsClient(
                new Uri(_endpoint!),
                new AzureKeyCredential(_apiKey!));
        }

        var generator = client.AsIEmbeddingGenerator(_modelId!);

        if (_retryPolicy != null)
        {
            return new RetryEmbeddingGenerator(generator, _retryPolicy);
        }

        return generator;
    }
}