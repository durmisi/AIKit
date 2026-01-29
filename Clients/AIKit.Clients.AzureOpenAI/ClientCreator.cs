using Azure;
using Azure.AI.Inference;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using System.Net;

namespace AIKit.Clients.AzureOpenAI;

/// <summary>
/// Internal creator for Azure OpenAI clients with shared configuration logic.
/// </summary>
internal static class ClientCreator
{
    /// <summary>
    /// Creates a ChatCompletionsClient from the provided settings.
    /// </summary>
    /// <param name="endpoint">The Azure OpenAI endpoint.</param>
    /// <param name="apiKey">The API key (optional if using credential).</param>
    /// <param name="useDefaultAzureCredential">Whether to use default Azure credential.</param>
    /// <param name="tokenCredential">Custom token credential.</param>
    /// <param name="httpClient">Optional pre-configured HttpClient.</param>
    /// <param name="proxy">Optional web proxy.</param>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <param name="userAgent">Optional user agent.</param>
    /// <param name="customHeaders">Optional custom headers.</param>
    /// <returns>The configured ChatCompletionsClient.</returns>
    internal static ChatCompletionsClient CreateChatCompletionsClient(
        string endpoint,
        string? apiKey = null,
        bool useDefaultAzureCredential = false,
        TokenCredential? tokenCredential = null,
        HttpClient? httpClient = null,
        IWebProxy? proxy = null,
        int timeoutSeconds = 30,
        string? userAgent = null,
        Dictionary<string, string>? customHeaders = null)
    {
        var options = new AzureAIInferenceClientOptions();

        if (httpClient == null)
        {
            var handler = new HttpClientHandler();
            if (proxy != null)
            {
                handler.Proxy = proxy;
            }
            httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
        }
        else if (proxy != null)
        {
            // HttpClient provided, proxy ignored
        }

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

        options.Transport = new HttpClientTransport(httpClient);

        ChatCompletionsClient client;

        if (tokenCredential != null)
        {
            client = new ChatCompletionsClient(
                new Uri(endpoint),
                tokenCredential,
                options);
        }
        else if (useDefaultAzureCredential)
        {
            client = new ChatCompletionsClient(
                new Uri(endpoint),
                new DefaultAzureCredential(),
                options);
        }
        else
        {
            client = new ChatCompletionsClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey!),
                options);
        }

        return client;
    }

    /// <summary>
    /// Creates an EmbeddingsClient from the provided settings.
    /// </summary>
    /// <param name="endpoint">The Azure OpenAI endpoint.</param>
    /// <param name="apiKey">The API key (optional if using credential).</param>
    /// <param name="useDefaultAzureCredential">Whether to use default Azure credential.</param>
    /// <param name="tokenCredential">Custom token credential.</param>
    /// <param name="httpClient">Optional pre-configured HttpClient.</param>
    /// <param name="proxy">Optional web proxy.</param>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <param name="userAgent">Optional user agent.</param>
    /// <param name="customHeaders">Optional custom headers.</param>
    /// <returns>The configured EmbeddingsClient.</returns>
    internal static EmbeddingsClient CreateEmbeddingsClient(
        string endpoint,
        string? apiKey = null,
        bool useDefaultAzureCredential = false,
        TokenCredential? tokenCredential = null,
        HttpClient? httpClient = null,
        IWebProxy? proxy = null,
        int timeoutSeconds = 30,
        string? userAgent = null,
        Dictionary<string, string>? customHeaders = null)
    {
        var options = new AzureAIInferenceClientOptions();

        if (httpClient == null)
        {
            var handler = new HttpClientHandler();
            if (proxy != null)
            {
                handler.Proxy = proxy;
            }
            httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
        }
        else if (proxy != null)
        {
            // HttpClient provided, proxy ignored
        }

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

        options.Transport = new HttpClientTransport(httpClient);

        EmbeddingsClient client;

        if (tokenCredential != null)
        {
            client = new EmbeddingsClient(
                new Uri(endpoint),
                tokenCredential,
                options);
        }
        else if (useDefaultAzureCredential)
        {
            client = new EmbeddingsClient(
                new Uri(endpoint),
                new DefaultAzureCredential(),
                options);
        }
        else
        {
            client = new EmbeddingsClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey!),
                options);
        }

        return client;
    }
}

// Generated by Copilot