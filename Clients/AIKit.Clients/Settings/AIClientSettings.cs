namespace AIKit.Clients.Settings;

/// <summary>
/// Settings for configuring AI client connections.
/// </summary>
public sealed class AIClientSettings
{
    /// <summary>
    /// The API key for authenticating with the AI provider.
    /// Not required for Ollama (local) or AWS Bedrock.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// The endpoint URL for the AI service.
    /// </summary>
    /// <remarks>
    /// Required for:
    /// <list type="bullet">
    /// <item>Azure OpenAI: Your Azure OpenAI resource endpoint</item>
    /// <item>Ollama: http://localhost:11434 (default)</item>
    /// <item>Claude: https://api.anthropic.com/v1/</item>
    /// <item>Mistral: https://api.mistral.ai/v1/</item>
    /// <item>Groq: https://api.groq.com/openai/v1/</item>
    /// <item>Azure Claude: Your Azure AI Foundry endpoint (e.g., https://your-endpoint.services.ai.azure.com)</item>
    /// </list>
    /// Not required for OpenAI, Gemini, AWS Bedrock, or GitHub Models.
    /// </remarks>
    public string? Endpoint { get; init; }

    /// <summary>
    /// The model ID/name to use.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// <list type="bullet">
    /// <item>OpenAI: "gpt-4o", "gpt-4o-mini", "text-embedding-3-small"</item>
    /// <item>Azure OpenAI: Your deployment name</item>
    /// <item>Ollama: "llama3.2", "mistral", "nomic-embed-text"</item>
    /// <item>Gemini: "gemini-2.5-flash", "gemini-2.5-pro", "text-embedding-004"</item>
    /// <item>Claude: "claude-3-5-sonnet-20241022", "claude-3-haiku-20240307"</item>
    /// <item>Mistral: "mistral-large-latest", "mistral-embed"</item>
    /// <item>Groq: "llama-3.3-70b-versatile", "mixtral-8x7b-32768"</item>
    /// <item>Bedrock Chat: "anthropic.claude-v2.1", "meta.llama3-70b-instruct-v1:0"</item>
    /// <item>Bedrock Embeddings: "amazon.titan-embed-text-v1", "amazon.titan-embed-text-v2:0", "cohere.embed-english-v3"</item>
    /// <item>GitHub Models: "gpt-4.1-mini", "openai/gpt-4o", "meta-llama/Llama-3.3-70B-Instruct"</item>
    /// <item>Azure Claude: "claude-sonnet-4-5", "claude-haiku-4-5", "claude-opus-4-1"</item>
    /// </list>
    /// </remarks>
    public string? ModelId { get; init; }

    /// <summary>
    /// Retry policy settings for handling transient failures.
    /// </summary>
    public RetryPolicySettings? RetryPolicy { get; init; }

    /// <summary>
    /// The name of the provider (e.g., "open-ai", "azure-open-ai").
    /// </summary>
    public string? ProviderName { get; init; }

    /// <summary>
    /// Organization ID for OpenAI.
    /// </summary>
    public string? Organization { get; init; }

    /// <summary>
    /// Custom HttpClient for requests.
    /// </summary>
    public HttpClient? HttpClient { get; init; }

    /// <summary>
    /// AWS access key for Bedrock.
    /// </summary>
    public string? AwsAccessKey { get; init; }

    /// <summary>
    /// AWS secret key for Bedrock.
    /// </summary>
    public string? AwsSecretKey { get; init; }

    /// <summary>
    /// AWS region for Bedrock.
    /// </summary>
    public string? AwsRegion { get; init; }

    /// <summary>
    /// GitHub token for GitHub Models.
    /// </summary>
    public string? GitHubToken { get; init; }

    /// <summary>
    /// Whether to use default Azure credential for Azure OpenAI.
    /// </summary>
    public bool UseDefaultAzureCredential { get; init; }
}

/// <summary>
/// Settings for retry policy.
/// </summary>
public sealed class RetryPolicySettings
{
    /// <summary>
    /// Maximum number of retry attempts. Default is 3.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Initial delay between retries in seconds. Default is 1.
    /// </summary>
    public double InitialDelaySeconds { get; init; } = 1.0;

    /// <summary>
    /// Maximum delay between retries in seconds. Default is 60.
    /// </summary>
    public double MaxDelaySeconds { get; init; } = 60.0;

    /// <summary>
    /// Backoff multiplier for exponential backoff. Default is 2.0.
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;
}

public static class AIClientSettingsValidator
{
    /// <summary>
    /// Validates that the API key is provided in the settings.
    /// </summary>
    /// <param name="settings">The AI client settings to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if settings is null.</exception>
    /// <exception cref="ArgumentException">Thrown if ApiKey is null or whitespace.</exception>
    public static void RequireApiKey(AIClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new ArgumentException(
                "ApiKey is required.",
                nameof(AIClientSettings.ApiKey));
    }

    /// <summary>
    /// Validates that the endpoint is provided and is a valid absolute URI in the settings.
    /// </summary>
    /// <param name="settings">The AI client settings to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if settings is null.</exception>
    /// <exception cref="ArgumentException">Thrown if Endpoint is null, whitespace, or not a valid URI.</exception>
    public static void RequireEndpoint(AIClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.Endpoint))
            throw new ArgumentException(
                "Endpoint is required.",
                nameof(AIClientSettings.Endpoint));

        if (!Uri.TryCreate(settings.Endpoint, UriKind.Absolute, out _))
            throw new ArgumentException(
                "Endpoint must be a valid absolute URI.",
                nameof(AIClientSettings.Endpoint));
    }

    /// <summary>
    /// Validates that the model ID is provided in the settings.
    /// </summary>
    /// <param name="settings">The AI client settings to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if settings is null.</exception>
    /// <exception cref="ArgumentException">Thrown if ModelId is null or whitespace.</exception>
    public static void RequireModel(AIClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.ModelId))
            throw new ArgumentException(
                "ModelId is required.",
                nameof(AIClientSettings.ModelId));
    }
}