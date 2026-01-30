using AIKit.Clients.Resilience;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.GitHub;

/// <summary>
/// Builder for creating GitHub Models embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorBuilder 
{
    private string? _model;
    private HttpClient? _httpClient;
    private string? _gitHubToken;
    private RetryPolicySettings? _retryPolicy;
    private ILogger<EmbeddingGeneratorBuilder>? _logger;
    private string? _organizationId;
    private string? _projectId;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorBuilder"/>.
    /// </summary>
    public EmbeddingGeneratorBuilder()
    {
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => "github-models";

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
    /// Sets the model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithModel(string model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        return this;
    }

    /// <summary>
    /// Sets the HTTP client.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
    /// Sets the logger.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithLogger(ILogger<EmbeddingGeneratorBuilder> logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    /// Sets the organization ID.
    /// </summary>
    /// <param name="organizationId">The organization identifier.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithOrganizationId(string organizationId)
    {
        _organizationId = organizationId;
        return this;
    }

    /// <summary>
    /// Sets the project ID.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithProjectId(string projectId)
    {
        _projectId = projectId;
        return this;
    }

    /// <summary>
    /// Builds the IEmbeddingGenerator instance.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Build()
    {
        Validate();

        var client = ClientCreator.CreateOpenAIClient(
            _gitHubToken!, _organizationId, _projectId, Constants.GitHubModelsEndpoint, _httpClient);

        var targetModel = _model!;
        _logger?.LogInformation("Creating GitHub Models embedding generator for model {Model}", targetModel);

        var embeddingClient = client.GetEmbeddingClient(targetModel).AsIEmbeddingGenerator();

        if (_retryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", _retryPolicy.MaxRetries);
            return new RetryEmbeddingGenerator(embeddingClient, _retryPolicy);
        }

        return embeddingClient;
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_gitHubToken))
            throw new ArgumentException("GitHubToken is required.", nameof(_gitHubToken));

        if (string.IsNullOrWhiteSpace(_model))
            throw new ArgumentException("Model is required.", nameof(_model));
    }
}