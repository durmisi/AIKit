using AIKit.Clients.Resilience;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Bedrock;

/// <summary>
/// Builder for creating Bedrock chat clients with maximum flexibility.
/// </summary>
public class ChatClientBuilder
{
    private string? _modelId;
    private string? _awsAccessKey;
    private string? _awsSecretKey;
    private string? _awsRegion;
    private AWSCredentials? _awsCredentials;
    private RetryPolicySettings? _retryPolicy;
    private int _timeoutSeconds = 30;
    private ILogger<ChatClientBuilder>? _logger;
    private int? _maxRetries;
    private string? _serviceUrl;

    /// <summary>
    /// Sets the model ID.
    /// </summary>
    /// <param name="modelId">The model identifier.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithModel(string modelId)
    {
        _modelId = modelId;
        return this;
    }

    /// <summary>
    /// Sets the AWS access key.
    /// </summary>
    /// <param name="awsAccessKey">The AWS access key.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithAwsAccessKey(string awsAccessKey)
    {
        _awsAccessKey = awsAccessKey;
        _awsCredentials = null;
        return this;
    }

    /// <summary>
    /// Sets the AWS secret key.
    /// </summary>
    /// <param name="awsSecretKey">The AWS secret key.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithAwsSecretKey(string awsSecretKey)
    {
        _awsSecretKey = awsSecretKey;
        _awsCredentials = null;
        return this;
    }

    /// <summary>
    /// Sets the AWS region.
    /// </summary>
    /// <param name="awsRegion">The AWS region.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithAwsRegion(string awsRegion)
    {
        _awsRegion = awsRegion;
        return this;
    }

    /// <summary>
    /// Sets the AWS credentials.
    /// </summary>
    /// <param name="credentials">The AWS credentials.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithAwsCredentials(AWSCredentials credentials)
    {
        _awsCredentials = credentials;
        _awsAccessKey = null;
        _awsSecretKey = null;
        return this;
    }

    /// <summary>
    /// Sets the retry policy.
    /// </summary>
    /// <param name="retryPolicy">The retry policy settings.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithRetryPolicy(RetryPolicySettings retryPolicy)
    {
        _retryPolicy = retryPolicy;
        return this;
    }

    /// <summary>
    /// Sets the timeout in seconds.
    /// </summary>
    /// <param name="seconds">The timeout value.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithTimeout(int seconds)
    {
        _timeoutSeconds = seconds;
        return this;
    }

    /// <summary>
    /// Sets the logger.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithLogger(ILogger<ChatClientBuilder> logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    /// Sets the maximum retries.
    /// </summary>
    /// <param name="maxRetries">The maximum number of retries.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithMaxRetries(int maxRetries)
    {
        _maxRetries = maxRetries;
        return this;
    }

    /// <summary>
    /// Sets the service URL.
    /// </summary>
    /// <param name="serviceUrl">The service URL.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithServiceUrl(string serviceUrl)
    {
        _serviceUrl = serviceUrl;
        return this;
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => GetDefaultProviderName();

    /// <summary>
    /// Builds the IChatClient instance.
    /// </summary>
    /// <returns>The created chat client.</returns>
    public IChatClient Build()
    {
        Validate();

        var runtime = ClientCreator.CreateBedrockRuntimeClient(
            _awsRegion!, _awsAccessKey, _awsSecretKey, _awsCredentials, _timeoutSeconds, _maxRetries, _serviceUrl);

        var targetModel = _modelId!;
        _logger?.LogInformation("Creating AWS Bedrock chat client for model {Model} in region {Region}", targetModel, _awsRegion);

        var client = runtime.AsIChatClient(targetModel);

        if (_retryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", _retryPolicy.MaxRetries);
            return new RetryChatClient(client, _retryPolicy);
        }

        return client;
    }

    private string GetDefaultProviderName() => "aws-bedrock";

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_awsRegion))
            throw new ArgumentException("AwsRegion is required.", nameof(_awsRegion));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));

        if (_awsCredentials == null && (string.IsNullOrWhiteSpace(_awsAccessKey) || string.IsNullOrWhiteSpace(_awsSecretKey)))
            throw new ArgumentException("Either AwsCredentials or both AwsAccessKey and AwsSecretKey are required.", nameof(_awsAccessKey));
    }
}

