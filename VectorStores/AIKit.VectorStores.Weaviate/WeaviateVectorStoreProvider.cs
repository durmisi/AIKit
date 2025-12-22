using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Weaviate;

namespace AIKit.VectorStores.Weaviate;

public sealed class WeaviateVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "weaviate";

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var client = CreateHttpClient(settings);

        var options = new WeaviateVectorStoreOptions
        {
            EmbeddingGenerator = ResolveEmbeddingGenerator(settings),
        };

        return new WeaviateVectorStore(client, options);
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

    private static IEmbeddingGenerator? ResolveEmbeddingGenerator(
        VectorStoreSettings settings)
    {
        return VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings);
    }
}