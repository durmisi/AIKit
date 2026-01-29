using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Bedrock;

/// <summary>
/// Builder for creating Bedrock embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorBuilder 
{
    private string? _awsAccessKey;
    private string? _awsSecretKey;
    private string? _awsRegion;
    private string? _modelId;
    private AWSCredentials? _awsCredentials;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorBuilder"/>.
    /// </summary>
    public EmbeddingGeneratorBuilder()
    {
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => "aws-bedrock";

    /// <summary>
    /// Sets the AWS access key.
    /// </summary>
    /// <param name="awsAccessKey">The AWS access key.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithAwsAccessKey(string? awsAccessKey)
    {
        _awsAccessKey = awsAccessKey ?? throw new ArgumentNullException(nameof(awsAccessKey));
        _awsCredentials = null;
        return this;
    }

    /// <summary>
    /// Sets the AWS secret key.
    /// </summary>
    /// <param name="awsSecretKey">The AWS secret key.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithAwsSecretKey(string? awsSecretKey)
    {
        _awsSecretKey = awsSecretKey ?? throw new ArgumentNullException(nameof(awsSecretKey));
        _awsCredentials = null;
        return this;
    }

    /// <summary>
    /// Sets the AWS region.
    /// </summary>
    /// <param name="awsRegion">The AWS region.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithAwsRegion(string? awsRegion)
    {
        _awsRegion = awsRegion ?? throw new ArgumentNullException(nameof(awsRegion));
        return this;
    }

    /// <summary>
    /// Sets the AWS credentials.
    /// </summary>
    /// <param name="credentials">The AWS credentials.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithAwsCredentials(AWSCredentials credentials)
    {
        _awsCredentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        _awsAccessKey = null;
        _awsSecretKey = null;
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
        if (_awsCredentials == null && (string.IsNullOrWhiteSpace(_awsAccessKey) || string.IsNullOrWhiteSpace(_awsSecretKey)))
            throw new InvalidOperationException("Either AwsCredentials or both AwsAccessKey and AwsSecretKey are required. Call WithAwsCredentials() or WithAwsAccessKey() and WithAwsSecretKey().");

        if (string.IsNullOrWhiteSpace(_awsRegion))
            throw new InvalidOperationException("AwsRegion is required. Call WithAwsRegion().");

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new InvalidOperationException("ModelId is required. Call WithModelId().");

        var runtime = ClientCreator.CreateBedrockRuntimeClient(_awsRegion, _awsAccessKey, _awsSecretKey, _awsCredentials);

        return runtime.AsIEmbeddingGenerator(_modelId);
    }
}