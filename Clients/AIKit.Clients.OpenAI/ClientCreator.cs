using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net;

namespace AIKit.Clients.OpenAI;

/// <summary>
/// Internal creator for OpenAI clients with shared configuration logic.
/// </summary>
internal static class ClientCreator
{
    /// <summary>
    /// Creates an OpenAIClient from the provided settings.
    /// </summary>
    /// <param name="apiKey">The API key for authentication.</param>
    /// <param name="organization">Optional organization ID.</param>
    /// <param name="projectId">Optional project ID.</param>
    /// <param name="endpoint">Optional custom endpoint URL.</param>
    /// <param name="httpClient">Optional pre-configured HttpClient.</param>
    /// <param name="proxy">Optional web proxy.</param>
    /// <param name="timeoutSeconds">Timeout in seconds (default: 30).</param>
    /// <returns>The configured OpenAIClient.</returns>
    internal static OpenAIClient CreateClient(
        string apiKey,
        string? organization = null,
        string? projectId = null,
        string? endpoint = null,
        HttpClient? httpClient = null,
        IWebProxy? proxy = null,
        int timeoutSeconds = 30)
    {
        var options = new OpenAIClientOptions();

        if (!string.IsNullOrEmpty(endpoint))
        {
            options.Endpoint = new Uri(endpoint);
        }

        if (!string.IsNullOrWhiteSpace(organization))
        {
            options.OrganizationId = organization;
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

