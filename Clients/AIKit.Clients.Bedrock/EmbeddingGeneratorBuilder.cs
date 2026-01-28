using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Bedrock;

/// <summary>
/// Builder for creating Bedrock embedding generators with maximum flexibility.
/// </summary>
public class EmbeddingGeneratorBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private string? _awsAccessKey;
    private string? _awsSecretKey;
    private string? _awsRegion;
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
    /// Sets the AWS access key.
    /// </summary>
    /// <param name="awsAccessKey">The AWS access key.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithAwsAccessKey(string awsAccessKey)
    {
        _awsAccessKey = awsAccessKey;
        return this;
    }

    /// <summary>
    /// Sets the AWS secret key.
    /// </summary>
    /// <param name="awsSecretKey">The AWS secret key.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithAwsSecretKey(string awsSecretKey)
    {
        _awsSecretKey = awsSecretKey;
        return this;
    }

    /// <summary>
    /// Sets the AWS region.
    /// </summary>
    /// <param name="awsRegion">The AWS region.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithAwsRegion(string awsRegion)
    {
        _awsRegion = awsRegion;
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
        var settings = new Dictionary<string, object>
        {
            ["ApiKey"] = _apiKey,
            ["ModelId"] = _modelId!,
            ["AwsAccessKey"] = _awsAccessKey!,
            ["AwsSecretKey"] = _awsSecretKey!,
            ["AwsRegion"] = _awsRegion!,
            ["RetryPolicy"] = _retryPolicy
        };

        var factory = new EmbeddingGeneratorFactory(settings);
        return factory.Create();
    }
}