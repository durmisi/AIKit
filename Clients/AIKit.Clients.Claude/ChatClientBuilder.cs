using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Claude;

/// <summary>
/// Builder for creating Claude chat clients with maximum flexibility.
/// </summary>
public class ChatClientBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private int _timeoutSeconds = 30;
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

    private string GetDefaultProviderName() => "claude";

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new ArgumentException("ApiKey is required.", nameof(_apiKey));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));
    }

    private IChatClient CreateClient(string? modelName)
    {
        AnthropicClient client = new(new Anthropic.Core.ClientOptions()
        {
            ApiKey = _apiKey,
            Timeout = TimeSpan.FromSeconds(_timeoutSeconds),
        });

        var targetModel = modelName ?? _modelId;
        IChatClient chatClient = client.AsIChatClient(targetModel)
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();

        return chatClient;
    }
}