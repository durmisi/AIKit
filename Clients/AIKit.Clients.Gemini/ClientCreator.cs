using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using System.Net;

namespace AIKit.Clients.Gemini;

/// <summary>
/// Internal creator for Gemini clients with shared configuration logic.
/// </summary>
internal static class ClientCreator
{
    /// <summary>
    /// Creates a GeminiChatClient from the provided settings.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="modelId">The model ID.</param>
    /// <param name="httpClient">Optional pre-configured HttpClient.</param>
    /// <param name="proxy">Optional web proxy.</param>
    /// <param name="timeoutSeconds">Timeout in seconds.</param>
    /// <param name="userAgent">Optional user agent.</param>
    /// <param name="customHeaders">Optional custom headers.</param>
    /// <returns>The configured GeminiChatClient.</returns>
    internal static GeminiChatClient CreateGeminiChatClient(
        string apiKey,
        string modelId,
        HttpClient? httpClient = null,
        IWebProxy? proxy = null,
        int timeoutSeconds = 30,
        string? userAgent = null,
        Dictionary<string, string>? customHeaders = null)
    {
        // Note: GeminiClientOptions doesn't support HttpClient, proxy, timeout, etc.
        var options = new GeminiClientOptions
        {
            ApiKey = apiKey,
            ModelId = modelId
        };

        return new GeminiChatClient(options);
    }

    /// <summary>
    /// Creates a GeminiEmbeddingGenerator from the provided settings.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="modelId">The model ID.</param>
    /// <returns>The configured GeminiEmbeddingGenerator.</returns>
    internal static GeminiEmbeddingGenerator CreateGeminiEmbeddingGenerator(
        string apiKey,
        string modelId)
    {
        var options = new GeminiClientOptions
        {
            ApiKey = apiKey,
            ModelId = modelId
        };

        return new GeminiEmbeddingGenerator(options);
    }
}

