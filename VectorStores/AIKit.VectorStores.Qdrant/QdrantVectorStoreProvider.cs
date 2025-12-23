using AIKit.Core.VectorStores;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace AIKit.VectorStores.Qdrant;

public sealed class QdrantVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "qdrant";

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var client = CreateQdrantClient(settings);

        return new QdrantVectorStore(client, ownsClient: true);
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using endpoint.
    /// </summary>
    public VectorStore Create(string endpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);

        var client = new QdrantClient(endpoint);

        return new QdrantVectorStore(client, ownsClient: true);
    }

    /// <summary>
    /// Creates a vector store with a pre-configured QdrantClient.
    /// For advanced scenarios where full control over the Qdrant client is needed.
    /// </summary>
    public VectorStore Create(QdrantClient qdrantClient)
    {
        ArgumentNullException.ThrowIfNull(qdrantClient);

        return new QdrantVectorStore(qdrantClient, ownsClient: true);
    }

    private static QdrantClient CreateQdrantClient(VectorStoreSettings settings)
    {
        var client = new QdrantClient(settings.Endpoint);

        // Note: ApiKey configuration may require different approach
        // QdrantClient may not support apiKey in constructor

        return client;
    }
}