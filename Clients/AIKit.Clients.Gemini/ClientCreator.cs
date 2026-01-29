using GeminiDotnet;
using GeminiDotnet.Extensions.AI;

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
    /// <returns>The configured GeminiChatClient.</returns>
    internal static GeminiChatClient CreateGeminiChatClient(
        string apiKey,
        string modelId)
    {
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

