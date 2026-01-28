using AIKit.Clients.Interfaces;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.GitHub;

/// <summary>
/// Builder for creating GitHub Models embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorBuilder 
{
    private string? _gitHubToken;
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
    public string Provider => "github";

    /// <summary>
    /// Sets the GitHub token.
    /// </summary>
    /// <param name="gitHubToken">The GitHub token.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithGitHubToken(string gitHubToken)
    {
        _gitHubToken = gitHubToken ?? throw new ArgumentNullException(nameof(gitHubToken));
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
        if (string.IsNullOrWhiteSpace(_gitHubToken))
            throw new InvalidOperationException("GitHubToken is required. Call WithGitHubToken().");

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new InvalidOperationException("ModelId is required. Call WithModelId().");

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://models.inference.ai.azure.com/") // Assuming Constants.GitHubModelsEndpoint
        };

        var credential = new ApiKeyCredential(_gitHubToken);
        var client = new OpenAIClient(credential, options);
        return client.GetEmbeddingClient(_modelId).AsIEmbeddingGenerator();
    }
}