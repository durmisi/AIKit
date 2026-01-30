using AIKit.Clients.Resilience;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.OpenAI;

/// <summary>
/// Builder for creating OpenAI embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorBuilder
{
    private string? _apiKey;
    private string? _model;
    private string? _organization;
    private RetryPolicySettings? _retryPolicy;

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
    /// Sets the organization ID.
    /// </summary>
    /// <param name="organization">The organization ID.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithOrganization(string organization)
    {
        _organization = organization ?? throw new ArgumentNullException(nameof(organization));
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
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("ApiKey is required. Call WithApiKey().");

        if (string.IsNullOrWhiteSpace(_model))
            throw new InvalidOperationException("Model is required. Call WithModel().");

        var client = ClientCreator.CreateClient(_apiKey!, _organization);
        var generator = client.GetEmbeddingClient(_model!).AsIEmbeddingGenerator();

        if (_retryPolicy != null)
        {
            return new RetryEmbeddingGenerator(generator, _retryPolicy);
        }

        return generator;
    }
}