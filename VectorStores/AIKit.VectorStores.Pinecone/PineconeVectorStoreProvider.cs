using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Pinecone;
using Pinecone;

namespace AIKit.VectorStores.Pinecone;

public sealed class PineconeVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "pinecone";

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ApiKey);

        var client = CreatePineconeClient(settings);

        var options = new PineconeVectorStoreOptions
        {
            EmbeddingGenerator = ResolveEmbeddingGenerator(settings),
        };

        return new PineconeVectorStore(client, options);
    }

    private static PineconeClient CreatePineconeClient(VectorStoreSettings settings)
    {
        string? environment = null;

        if (settings.AdditionalSettings != null &&
            settings.AdditionalSettings.TryGetValue("Environment", out var env) && env is string envStr)
        {
            environment = envStr;
        }

        return new PineconeClient(settings.ApiKey, environment);
    }

    private static IEmbeddingGenerator? ResolveEmbeddingGenerator(
        VectorStoreSettings settings)
    {
        return VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings);
    }
}