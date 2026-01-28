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
        => Create(BuildSettings(), modelName);

    /// <summary>
    /// Creates a chat client with the specified settings.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <param name="modelName">Optional model name to use for the client.</param>
    /// <returns>The created chat client.</returns>
    public IChatClient Create(AIClientSettings settings, string? modelName = null)
    {
        Validate(settings);

        var client = CreateClient(settings, modelName);

        if (settings.RetryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", settings.RetryPolicy.MaxRetries);
            return new RetryChatClient(client, settings.RetryPolicy);
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

    private AIClientSettings BuildSettings()
    {
        return new AIClientSettings
        {
            ApiKey = _apiKey,
            ModelId = _modelId,
            AwsAccessKey = _awsAccessKey,
            AwsSecretKey = _awsSecretKey,
            AwsRegion = _awsRegion,
            RetryPolicy = _retryPolicy,
            TimeoutSeconds = _timeoutSeconds
        };
    }

    private string GetDefaultProviderName() => "aws-bedrock";

    private void Validate(AIClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.AwsAccessKey))
            throw new ArgumentException(
                "AwsAccessKey is required.",
                nameof(AIClientSettings.AwsAccessKey));

        if (string.IsNullOrWhiteSpace(settings.AwsSecretKey))
            throw new ArgumentException(
                "AwsSecretKey is required.",
                nameof(AIClientSettings.AwsSecretKey));

        if (string.IsNullOrWhiteSpace(settings.AwsRegion))
            throw new ArgumentException(
                "AwsRegion is required.",
                nameof(AIClientSettings.AwsRegion));

        AIClientSettingsValidator.RequireModel(settings);
    }

    private IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
        var regionEndpoint = RegionEndpoint.GetBySystemName(settings.AwsRegion!);

        IAmazonBedrockRuntime runtime = new AmazonBedrockRuntimeClient(
            settings.AwsAccessKey!,
            settings.AwsSecretKey!,
            regionEndpoint);

        var targetModel = modelName ?? settings.ModelId;
        _logger?.LogInformation("Creating AWS Bedrock chat client for model {Model} in region {Region}", targetModel, settings.AwsRegion);

        return runtime.AsIChatClient(targetModel);
    }
}