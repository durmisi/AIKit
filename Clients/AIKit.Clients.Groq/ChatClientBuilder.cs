using AIKit.Clients.Resilience;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Groq;

/// <summary>
/// Builder for creating Groq chat clients with maximum flexibility.
/// </summary>
public class ChatClientBuilder
{
    private string? _apiKey;
    private string? _modelId;
    private HttpClient? _httpClient;
    private RetryPolicySettings? _retryPolicy;
    private ILogger<ChatClientBuilder>? _logger;
    private string? _organizationId;
    private string? _projectId;

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

        var client = ClientCreator.CreateOpenAIClient(
            _apiKey!, _organizationId, _projectId, ClientCreator.DefaultEndpoint, _httpClient);

        var targetModel = _modelId!;
        _logger?.LogInformation("Creating Groq chat client for model {Model}", targetModel);

        var chatClient = client.GetChatClient(targetModel).AsIChatClient();

        if (_retryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", _retryPolicy.MaxRetries);
            return new RetryChatClient(chatClient, _retryPolicy);
        }

        return chatClient;
    }

    private string GetDefaultProviderName() => "groq";

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new ArgumentException("ApiKey is required.", nameof(_apiKey));

        if (string.IsNullOrWhiteSpace(_modelId))
            throw new ArgumentException("ModelId is required.", nameof(_modelId));
    }
}

