using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Weaviate;
using System.Net.Http;

namespace AIKit.Core.Vector;

public sealed class WeaviateVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "weaviate";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "WeaviateVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var client = new HttpClient { BaseAddress = new Uri(settings.Endpoint) };

        var options = new WeaviateVectorStoreOptions
        {
            EmbeddingGenerator = ResolveEmbeddingGenerator(settings),
        };

        return new WeaviateVectorStore(client, options);
    }

    private static IEmbeddingGenerator? ResolveEmbeddingGenerator(
        VectorStoreSettings settings)
    {
        return settings.EmbeddingGenerator ?? throw new InvalidOperationException(
            "An IEmbeddingGenerator must be provided in VectorStoreSettings for WeaviateVectorStoreProvider.");
    }
}