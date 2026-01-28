using AIKit.Clients.AzureOpenAI;
using AIKit.Clients.Base;
using AIKit.Clients.Interfaces;
using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Azure;
using Azure.AI.Inference;
using Azure.Identity;
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
    private RetryPolicySettings? _retryPolicy;
    private HttpClient? _httpClient;
    private int _timeoutSeconds = 30;
    private ILogger<ChatClientBuilder>? _logger;

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
            Endpoint = _endpoint,
            ModelId = _modelId,
            ApiKey = _apiKey,
            UseDefaultAzureCredential = _useDefaultAzureCredential,
            RetryPolicy = _retryPolicy,
            HttpClient = _httpClient,
            TimeoutSeconds = _timeoutSeconds
        };
    }

    private string GetDefaultProviderName() => "azure-open-ai";

    private void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);

        if (!settings.UseDefaultAzureCredential)
        {
            AIClientSettingsValidator.RequireApiKey(settings);
        }
    }

    private IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
        ChatCompletionsClient? client = null;

        if (settings.UseDefaultAzureCredential)
        {
            client = new ChatCompletionsClient(
                    new Uri(settings.Endpoint!),
                    new DefaultAzureCredential());
        }
        else
        {
            client = new ChatCompletionsClient(
                new Uri(settings.Endpoint!),
                new AzureKeyCredential(settings.ApiKey!));
        }

        var targetModel = modelName ?? settings.ModelId;
        _logger?.LogInformation("Creating Azure OpenAI chat client for model {Model}", targetModel);

        return client.AsIChatClient(targetModel);
    }
}