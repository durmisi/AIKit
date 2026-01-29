using AIKit.Clients.Resilience;
using Azure.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.AzureOpenAI;

/// <summary>
/// Builder for creating Azure OpenAI chat clients with maximum flexibility.
/// </summary>
public class ChatClientBuilder
{
    private string? _endpoint;
    private string? _modelId;
    private string? _apiKey;
    private bool _useDefaultAzureCredential;
    private TokenCredential? _tokenCredential;
    private RetryPolicySettings? _retryPolicy;
    private HttpClient? _httpClient;
    private ILogger<ChatClientBuilder>? _logger;
    private string? _userAgent;
    private Dictionary<string, string>? _customHeaders;

    /// <summary>
    /// Sets the Azure OpenAI endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint URL.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithEndpoint(string endpoint)
    {
        _endpoint = endpoint;
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
    /// Sets the API key for authentication.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey;
        _useDefaultAzureCredential = false;
        _tokenCredential = null;
        return this;
    }

    /// <summary>
    /// Configures to use default Azure credential for authentication.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithDefaultAzureCredential()
    {
        _useDefaultAzureCredential = true;
        _apiKey = null;
        _tokenCredential = null;
        return this;
    }

    /// <summary>
    /// Configures to use a custom Azure token credential for authentication.
    /// </summary>
    /// <param name="credential">The token credential.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithTokenCredential(TokenCredential credential)
    {
        _tokenCredential = credential;
        _useDefaultAzureCredential = false;
        _apiKey = null;
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
    /// Sets the user agent.
    /// </summary>
    /// <param name="userAgent">The user agent string.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithUserAgent(string userAgent)
    {
        _userAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Sets custom headers.
    /// </summary>
    /// <param name="headers">The custom headers.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithCustomHeaders(Dictionary<string, string> headers)
    {
        _customHeaders = headers;
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
        _logger?.LogInformation("Creating Azure OpenAI chat client for model {Model}", targetModel);

        var client = ClientCreator.CreateChatCompletionsClient(
            _endpoint!, _apiKey, _useDefaultAzureCredential, _tokenCredential, _httpClient, _userAgent, _customHeaders);

        var chatClient = client.AsIChatClient(targetModel);

        if (_retryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", _retryPolicy.MaxRetries);
            return new RetryChatClient(chatClient, _retryPolicy);
        }

        return chatClient;
    }

    private string GetDefaultProviderName() => "azure-open-ai";

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
            throw new ArgumentException("Endpoint is required.", nameof(_endpoint));

        if (!Uri.TryCreate(_endpoint, UriKind.Absolute, out _))
            throw new ArgumentException("Endpoint must be a valid absolute URI.", nameof(_endpoint));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));

        if (_tokenCredential == null && !_useDefaultAzureCredential && string.IsNullOrWhiteSpace(_apiKey))
            throw new ArgumentException("Either ApiKey, DefaultAzureCredential, or TokenCredential is required.", nameof(_apiKey));
    }
}

