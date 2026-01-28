using AIKit.Clients.GitHub;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.GitHub;

/// <summary>
/// Builder for creating GitHub embedding generators with maximum flexibility.
/// </summary>
public class EmbeddingGeneratorBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private string? _gitHubToken;

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
    /// Builds the IEmbeddingGenerator instance.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Build()
    {
        if (string.IsNullOrWhiteSpace(_gitHubToken))
            throw new ArgumentException("GitHubToken is required.", nameof(_gitHubToken));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));

        var settings = new Dictionary<string, object>
        {
            ["GitHubToken"] = _gitHubToken,
            ["ModelId"] = _modelId
        };

        var factory = new EmbeddingGeneratorFactory(settings);
        return factory.Create();
    }
}