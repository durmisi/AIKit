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

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ApiKey);

        var client = CreatePineconeClient(settings);

        var options = new PineconeVectorStoreOptions
        {
            EmbeddingGenerator = VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings),
        };

        return new PineconeVectorStore(client, options);
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using API key and embedding generator.
    /// </summary>
    public VectorStore Create(string apiKey, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var client = new PineconeClient(apiKey);

        var options = new PineconeVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };

        return new PineconeVectorStore(client, options);
    }

    /// <summary>
    /// Creates a vector store with API key, environment, and embedding generator.
    /// </summary>
    public VectorStore Create(string apiKey, string environment, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(environment);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var client = new PineconeClient(apiKey, environment);

        var options = new PineconeVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };

        return new PineconeVectorStore(client, options);
    }

    /// <summary>
    /// Creates a vector store with a pre-configured PineconeClient and options.
    /// For advanced scenarios where full control over the Pinecone client is needed.
    /// </summary>
    public VectorStore Create(PineconeClient pineconeClient, PineconeVectorStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(pineconeClient);
        ArgumentNullException.ThrowIfNull(options);

        return new PineconeVectorStore(pineconeClient, options);
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
}