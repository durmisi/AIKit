using AIKit.Clients.Mistral;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Mistral;

/// <summary>
/// Builder for creating Mistral embedding generators with maximum flexibility.
/// </summary>
public class EmbeddingGeneratorBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private string? _gitHubToken;
    private RetryPolicySettings? _retryPolicy;

    /// <summary>
    /// Sets the API key.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey;
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
    /// Sets the GitHub token.
    /// </summary>
    /// <param name="gitHubToken">The GitHub token.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithGitHubToken(string gitHubToken)
    {
        _gitHubToken = gitHubToken;
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
            ApiKey = _apiKey,
            ModelId = _modelId,
            GitHubToken = _gitHubToken,
            RetryPolicy = _retryPolicy
        };

        var factory = new EmbeddingGeneratorFactory(settings);
        return factory.Create();
    }
}