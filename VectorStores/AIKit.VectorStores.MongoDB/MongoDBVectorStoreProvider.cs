using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using MongoDB.Driver;

namespace AIKit.VectorStores.MongoDB;

public sealed class MongoDBVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "mongodb";

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        var mongoClient = new MongoClient(settings.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName ?? "vectorstore");

        var options = new MongoVectorStoreOptions
        {
            EmbeddingGenerator = VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings),
        };

        return new MongoVectorStore(mongoDatabase, options);
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using connection string and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var mongoClient = new MongoClient(connectionString);
        var mongoDatabase = mongoClient.GetDatabase("vectorstore");

        var options = new MongoVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };

        return new MongoVectorStore(mongoDatabase, options);
    }

    /// <summary>
    /// Creates a vector store with connection string, database name, and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, string databaseName, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var mongoClient = new MongoClient(connectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseName);

        var options = new MongoVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };

        return new MongoVectorStore(mongoDatabase, options);
    }

    /// <summary>
    /// Creates a vector store with a pre-configured MongoDatabase and options.
    /// For advanced scenarios where full control over the MongoDB connection is needed.
    /// </summary>
    public VectorStore Create(IMongoDatabase mongoDatabase, MongoVectorStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(mongoDatabase);
        ArgumentNullException.ThrowIfNull(options);

        return new MongoVectorStore(mongoDatabase, options);
    }
}