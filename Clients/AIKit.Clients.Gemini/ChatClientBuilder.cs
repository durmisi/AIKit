using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AIKit.Clients.Gemini;

/// <summary>
/// Builder for creating Gemini chat clients with maximum flexibility.
/// </summary>
public class ChatClientBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private RetryPolicySettings? _retryPolicy;
    private int _timeoutSeconds = 30;
    private ILogger<ChatClientBuilder>? _logger;
    private HttpClient? _httpClient;
    private string? _userAgent;
    private IWebProxy? _proxy;
    private Dictionary<string, string>? _customHeaders;

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
    /// Sets the proxy.
    /// </summary>
    /// <param name="proxy">The web proxy.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithProxy(IWebProxy proxy)
    {
        _proxy = proxy;
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

        var client = CreateClient();

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

    private IChatClient CreateClient()
    {
        if (_httpClient == null)
        {
            var handler = new HttpClientHandler();
            if (_proxy != null)
            {
                handler.Proxy = _proxy;
            }
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(_timeoutSeconds) };
        }
        else if (_proxy != null)
        {
            _logger?.LogWarning("HttpClient provided, proxy setting ignored.");
        }

        if (!string.IsNullOrEmpty(_userAgent))
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
        }

        if (_customHeaders != null)
        {
            foreach (var kvp in _customHeaders)
            {
                _httpClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
            }
        }

        var options = new GeminiClientOptions
        {
            ApiKey = _apiKey!,
            ModelId = _modelId!
        };

        if (string.IsNullOrWhiteSpace(_modelId)) throw new ArgumentException("ModelId is required.", nameof(_modelId));
        var targetModel = _modelId!;
        _logger?.LogInformation("Creating Gemini chat client for model {Model}", targetModel);

        return new GeminiChatClient(options);
    }
}

// Generated by Copilot