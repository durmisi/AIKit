using AIKit.Clients.Resilience;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Gemini;

/// <summary>
/// Builder for creating Gemini chat clients with maximum flexibility.
/// </summary>
public class ChatClientBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private RetryPolicySettings? _retryPolicy;
    private ILogger<ChatClientBuilder>? _logger;

    /// <summary>
    /// Sets the API key for authentication.
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
    /// Builds the IChatClient instance.
    /// </summary>
    /// <returns>The created chat client.</returns>
    public IChatClient Build()
    {
        Validate();

        var targetModel = _modelId!;
        _logger?.LogInformation("Creating Gemini chat client for model {Model}", targetModel);

        var client = ClientCreator.CreateGeminiChatClient(_apiKey!, _modelId!);

        if (_retryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", _retryPolicy.MaxRetries);
            return new RetryChatClient(client, _retryPolicy);
        }

        return client;
    }

    private string GetDefaultProviderName() => "gemini";

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new ArgumentException("ApiKey is required.", nameof(_apiKey));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));
    }
}

