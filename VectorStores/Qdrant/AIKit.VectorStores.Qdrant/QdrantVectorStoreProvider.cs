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

        var options = new QdrantVectorStoreOptions
        {
            EmbeddingGenerator = settings.EmbeddingGenerator ?? throw new InvalidOperationException(
                "An IEmbeddingGenerator must be provided in VectorStoreSettings for QdrantVectorStoreProvider"),
            HasNamedVectors = true,
        };

        return new QdrantVectorStore(client, ownsClient: true, options);
    }
}