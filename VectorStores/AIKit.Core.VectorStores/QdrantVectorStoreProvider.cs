using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace AIKit.Core.Vector;

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

        var client = new QdrantClient(settings.Endpoint);

        return new QdrantVectorStore(client, ownsClient: true);
    }

}