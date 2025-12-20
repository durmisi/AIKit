using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Pinecone;
using Pinecone;

namespace AIKit.Core.Vector;

public sealed class PineconeVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "pinecone";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "PineconeVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ApiKey);

        var client = new PineconeClient(settings.ApiKey);

        var options = new PineconeVectorStoreOptions
        {
            EmbeddingGenerator = ResolveEmbeddingGenerator(settings),
        };

        return new PineconeVectorStore(client, options);
    }

    private static IEmbeddingGenerator? ResolveEmbeddingGenerator(
        VectorStoreSettings settings)
    {
        return settings.EmbeddingGenerator ?? throw new InvalidOperationException(
            "An IEmbeddingGenerator must be provided in VectorStoreSettings for PineconeVectorStoreProvider.");
    }
}