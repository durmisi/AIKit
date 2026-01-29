using Microsoft.Extensions.AI;
using System.Net;

namespace AIKit.Clients.Ollama;

/// <summary>
/// Internal creator for Ollama clients with shared configuration logic.
/// </summary>
internal static class ClientCreator
{
    /// <summary>
    /// Creates an OllamaChatClient from the provided settings.
    /// </summary>
    /// <param name="endpoint">The Ollama endpoint.</param>
    /// <param name="modelId">Optional model ID.</param>
    /// <param name="httpClient">Optional pre-configured HttpClient.</param>
    /// <param name="userAgent">Optional user agent.</param>
    /// <param name="customHeaders">Optional custom headers.</param>
    /// <param name="proxy">Optional web proxy.</param>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <returns>The configured OllamaChatClient.</returns>
    internal static OllamaChatClient CreateOllamaChatClient(
        string endpoint,
        string? modelId = null,
        HttpClient? httpClient = null,
        string? userAgent = null,
        Dictionary<string, string>? customHeaders = null,
        IWebProxy? proxy = null,
        int timeoutSeconds = 30)
    {
        if (httpClient != null)
        {
            if (!string.IsNullOrEmpty(userAgent))
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
            }

            if (customHeaders != null)
            {
                foreach (var kvp in customHeaders)
                {
                    httpClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }
            }
        }

        var uri = new Uri(endpoint);
        return new OllamaChatClient(uri, modelId, httpClient);
    }
}

