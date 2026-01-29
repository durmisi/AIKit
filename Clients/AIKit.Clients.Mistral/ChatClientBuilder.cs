using AIKit.Clients.Resilience;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;

namespace AIKit.Clients.Mistral;

/// <summary>
/// Builder for creating Mistral chat clients with maximum flexibility.
/// </summary>
public class ChatClientBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private HttpClient? _httpClient;
    private RetryPolicySettings? _retryPolicy;
    private int _timeoutSeconds = 30;
    private ILogger<ChatClientBuilder>? _logger;
    private string? _organizationId;
    private string? _projectId;
    private IWebProxy? _proxy;

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
    /// Sets the organization ID.
    /// </summary>
    /// <param name="organizationId">The organization identifier.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithOrganizationId(string organizationId)
    {
        _organizationId = organizationId;
        return this;
    }

    /// <summary>
    /// Sets the project ID.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <returns>The builder instance.</returns>
    public ChatClientBuilder WithProjectId(string projectId)
    {
        _projectId = projectId;
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

    private string GetDefaultProviderName() => "mistral";

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new ArgumentException("ApiKey is required.", nameof(_apiKey));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));
    }

    private IChatClient CreateClient()
    {
        var client = ClientCreator.CreateOpenAIClient(
            _apiKey!, _organizationId, _projectId, "https://api.mistral.ai/v1/", _httpClient, _proxy, _timeoutSeconds);

        if (string.IsNullOrWhiteSpace(_modelId)) throw new ArgumentException("ModelId is required.", nameof(_modelId));
        var targetModel = _modelId!;
        _logger?.LogInformation("Creating Mistral chat client for model {Model}", targetModel);

        return client.GetChatClient(targetModel).AsIChatClient();
    }
}

