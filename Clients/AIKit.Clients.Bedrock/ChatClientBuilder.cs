using AIKit.Clients.Bedrock;
using AIKit.Clients.Base;
using AIKit.Clients.Interfaces;
using AIKit.Clients.Settings;
using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using AIKit.Clients.Resilience;

namespace AIKit.Clients.Bedrock;

/// <summary>
/// Builder for creating Bedrock chat clients with maximum flexibility.
/// </summary>
public class ChatClientBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private string? _awsAccessKey;
    private string? _awsSecretKey;
    private string? _awsRegion;
    private RetryPolicySettings? _retryPolicy;
    private int _timeoutSeconds = 30;
    private ILogger<ChatClientBuilder>? _logger;

    /// <summary>
    /// Sets the API key.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey;
        return this;
    }

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
    /// Gets the provider name.
    /// </summary>
    public string Provider => GetDefaultProviderName();

    /// <summary>
    /// Creates a chat client using the default settings.
    /// </summary>
    /// <param name="modelName">Optional model name to use for the client.</param>
    /// <returns>The created chat client.</returns>
    public IChatClient Create(string? modelName = null)
    {
        Validate();

        var client = CreateClient(modelName);

        if (_retryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", _retryPolicy.MaxRetries);
            return new RetryChatClient(client, _retryPolicy);
        }

        return client;
    }

    /// <summary>
    /// Builds the IChatClient instance.
    /// </summary>
    /// <returns>The created chat client.</returns>
    public IChatClient Build()
    {
        return Create();
    }

    private string GetDefaultProviderName() => "aws-bedrock";

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_awsAccessKey))
            throw new ArgumentException("AwsAccessKey is required.", nameof(_awsAccessKey));

        if (string.IsNullOrWhiteSpace(_awsSecretKey))
            throw new ArgumentException("AwsSecretKey is required.", nameof(_awsSecretKey));

        if (string.IsNullOrWhiteSpace(_awsRegion))
            throw new ArgumentException("AwsRegion is required.", nameof(_awsRegion));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));
    }

    private IChatClient CreateClient(string? modelName)
    {
        var regionEndpoint = RegionEndpoint.GetBySystemName(_awsRegion!);

        IAmazonBedrockRuntime runtime = new AmazonBedrockRuntimeClient(
            _awsAccessKey!,
            _awsSecretKey!,
            regionEndpoint);

        var targetModel = modelName ?? _modelId;
        _logger?.LogInformation("Creating AWS Bedrock chat client for model {Model} in region {Region}", targetModel, _awsRegion);

        return runtime.AsIChatClient(targetModel);
    }
}