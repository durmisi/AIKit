using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace AIKit.Clients.Groq;

/// <summary>
/// Internal creator for Groq clients with shared configuration logic.
/// </summary>
internal static class ClientCreator
{
    internal const string DefaultEndpoint = "https://api.groq.com/openai/v1/";

    /// <summary>
    /// Creates an OpenAIClient configured for Groq from the provided settings.
    /// </summary>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="organizationId">Optional organization ID.</param>
    /// <param name="projectId">Optional project ID.</param>
    /// <param name="endpoint">The endpoint URL (default: Groq endpoint).</param>
    /// <param name="httpClient">Optional pre-configured HttpClient.</param>
    /// <returns>The configured OpenAIClient.</returns>
    internal static OpenAIClient CreateOpenAIClient(
        string apiKey,
        string? organizationId = null,
        string? projectId = null,
        string endpoint = DefaultEndpoint,
        HttpClient? httpClient = null)
    {
        var options = new OpenAIClientOptions();

        if (!string.IsNullOrEmpty(endpoint))
        {
            options.Endpoint = new Uri(endpoint);
        }

        if (!string.IsNullOrWhiteSpace(organizationId))
        {
            options.OrganizationId = organizationId;
        }

        if (!string.IsNullOrEmpty(projectId))
        {
            options.ProjectId = projectId;
        }

        if (httpClient != null)
        {
            options.Transport = new HttpClientPipelineTransport(httpClient);
        }

        var credential = new ApiKeyCredential(apiKey);
        return new OpenAIClient(credential, options);
    }
}

