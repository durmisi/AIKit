using AIKit.Core.VectorStores;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace AIKit.VectorStores.Qdrant;

public sealed class QdrantVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "qdrant";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "QdrantVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var client = CreateQdrantClient(settings);

        return new QdrantVectorStore(client, ownsClient: true);
    }

    private static QdrantClient CreateQdrantClient(VectorStoreSettings settings)
    {
        var client = new QdrantClient(settings.Endpoint);

        // Note: ApiKey configuration may require different approach
        // QdrantClient may not support apiKey in constructor

        return client;
    }
}