using Azure.Core;
using Azure.Identity;
using elbruno.Extensions.AI.Claude;
using System.Net;

namespace AIKit.Clients.AzureClaude;

/// <summary>
/// Internal creator for Azure Claude clients with shared configuration logic.
/// </summary>
internal static class ClientCreator
{
    /// <summary>
    /// Creates an AzureClaudeClient from the provided settings.
    /// </summary>
    /// <param name="endpoint">The Azure endpoint.</param>
    /// <param name="modelId">The model ID.</param>
    /// <param name="apiKey">The API key (optional if using credential).</param>
    /// <param name="useDefaultAzureCredential">Whether to use default Azure credential.</param>
    /// <param name="tokenCredential">Custom token credential.</param>
    /// <param name="httpClient">Optional pre-configured HttpClient.</param>
    /// <param name="userAgent">Optional user agent.</param>
    /// <param name="customHeaders">Optional custom headers.</param>
    /// <returns>The configured AzureClaudeClient.</returns>
    internal static AzureClaudeClient CreateAzureClaudeClient(
        string endpoint,
        string modelId,
        string? apiKey = null,
        bool useDefaultAzureCredential = false,
        TokenCredential? tokenCredential = null,
        HttpClient? httpClient = null,
        string? userAgent = null,
        Dictionary<string, string>? customHeaders = null)
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

        if (tokenCredential != null)
        {
            return new AzureClaudeClient(uri, modelId, tokenCredential, httpClient);
        }
        else if (useDefaultAzureCredential)
        {
            var credential = new DefaultAzureCredential();
            return new AzureClaudeClient(uri, modelId, credential, httpClient);
        }
        else
        {
            return new AzureClaudeClient(uri, modelId, apiKey!, httpClient);
        }
    }
}

