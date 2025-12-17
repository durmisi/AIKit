namespace AIKit.Core.Clients;


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
    /// The organization ID (optional, used by OpenAI only).
    /// </summary>
    public string? Organization { get; init; }

    /// <summary>
    /// AWS Access Key ID (required for Bedrock).
    /// </summary>
    public string? AwsAccessKey { get; init; }

    /// <summary>
    /// AWS Secret Access Key (required for Bedrock).
    /// </summary>
    public string? AwsSecretKey { get; init; }

    /// <summary>
    /// AWS Region (required for Bedrock, e.g., "us-east-1").
    /// </summary>
    public string? AwsRegion { get; init; }

    /// <summary>
    /// GitHub Personal Access Token (required for GitHub Models).
    /// Create a token at https://github.com/settings/tokens with appropriate permissions.
    /// </summary>
    public string? GitHubToken { get; init; }

    /// <summary>
    /// When true, uses DefaultAzureCredential for authentication (for Azure Claude).
    /// This enables managed identity, Azure CLI, environment variables, etc.
    /// </summary>
    public bool UseDefaultAzureCredential { get; init; }

    /// <summary>
    /// Optional HTTP client for custom network configurations.
    /// Currently not used - reserved for future extensibility.
    /// </summary>
    public HttpClient? HttpClient { get; init; }
}

public static class AIClientSettingsValidator
{
    public static void RequireApiKey(AIClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new ArgumentException(
                "ApiKey is required.",
                nameof(AIClientSettings.ApiKey));
    }

    public static void RequireEndpoint(AIClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.Endpoint))
            throw new ArgumentException(
                "Endpoint is required.",
                nameof(AIClientSettings.Endpoint));
    }

    public static void RequireModel(AIClientSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.ModelId))
            throw new ArgumentException(
                "ModelId is required.",
                nameof(AIClientSettings.ModelId));
    }
}
