using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace AIKit.Clients.OpenAI;

/// <summary>
/// Builder for creating OpenAI chat clients with maximum flexibility.
/// </summary>
public class ChatClientBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private string? _endpoint;
    private string? _organization;
    private HttpClient? _httpClient;
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
    /// Sets the custom endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint URL.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithEndpoint(string endpoint)
    {
        _endpoint = endpoint;
        return this;
    }

    /// <summary>
    /// Sets the organization ID.
    /// </summary>
    /// <param name="organization">The organization identifier.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithOrganization(string organization)
    {
        _organization = organization;
        return this;
    }

    /// <summary>
    /// Sets the HTTP client.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
    /// Builds the IChatClient instance.
    /// </summary>
    /// <returns>The created chat client.</returns>
    public IChatClient Build()
    {
        Validate();

        var client = CreateClient();

        if (_retryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", _retryPolicy.MaxRetries);
            return new RetryChatClient(client, _retryPolicy);
        }

        return client;
    }

    private string GetDefaultProviderName() => "open-ai";

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new ArgumentException("ApiKey is required.", nameof(_apiKey));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));
    }

    private IChatClient CreateClient()
    {
        var options = new OpenAIClientOptions();

        if (!string.IsNullOrEmpty(_endpoint))
        {
            options.Endpoint = new Uri(_endpoint!);
        }

        if (!string.IsNullOrWhiteSpace(_organization))
        {
            options.OrganizationId = _organization;
        }

        if (_httpClient != null)
        {
            options.Transport = new HttpClientPipelineTransport(_httpClient);
        }
        else
        {
            options.NetworkTimeout = TimeSpan.FromSeconds(_timeoutSeconds);
        }

        var credential = new ApiKeyCredential(_apiKey!);
        var client = new OpenAIClient(credential, options);

        if (string.IsNullOrWhiteSpace(_modelId)) throw new ArgumentException("ModelId is required.", nameof(_modelId));
        var targetModel = _modelId!;
        _logger?.LogInformation("Creating OpenAI chat client for model {Model}", targetModel);

        return client
            .GetChatClient(targetModel)
            .AsIChatClient();
    }
}
// Generated by Copilot