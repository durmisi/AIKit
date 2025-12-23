using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Weaviate;

namespace AIKit.VectorStores.Weaviate;

public sealed class WeaviateVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "weaviate";

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var client = CreateHttpClient(settings);

        var options = new WeaviateVectorStoreOptions
        {
            EmbeddingGenerator = VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings),
        };

        return new WeaviateVectorStore(client, options);
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using endpoint and embedding generator.
    /// </summary>
    public VectorStore Create(string endpoint, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var client = new HttpClient { BaseAddress = new Uri(endpoint) };

        var options = new WeaviateVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };

        return new WeaviateVectorStore(client, options);
    }

    /// <summary>
    /// Creates a vector store with endpoint, API key, and embedding generator.
    /// </summary>
    public VectorStore Create(string endpoint, string apiKey, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var client = new HttpClient { BaseAddress = new Uri(endpoint) };
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var options = new WeaviateVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };

        return new WeaviateVectorStore(client, options);
    }

    /// <summary>
    /// Creates a vector store with a pre-configured HttpClient and options.
    /// For advanced scenarios where full control over the HTTP client is needed.
    /// </summary>
    public VectorStore Create(HttpClient httpClient, WeaviateVectorStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        return new WeaviateVectorStore(httpClient, options);
    }

    private static HttpClient CreateHttpClient(VectorStoreSettings settings)
    {
        var client = new HttpClient { BaseAddress = new Uri(settings.Endpoint) };

        // Apply additional settings
        if (settings.AdditionalSettings != null)
        {
            if (settings.AdditionalSettings.TryGetValue("ApiKey", out var apiKey) && apiKey is string apiKeyStr)
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKeyStr}");
            }

            if (settings.AdditionalSettings.TryGetValue("TimeoutSeconds", out var timeout) && timeout is int timeoutSec)
            {
                client.Timeout = TimeSpan.FromSeconds(timeoutSec);
            }

            // Add more settings as needed
        }

        return client;
    }
}