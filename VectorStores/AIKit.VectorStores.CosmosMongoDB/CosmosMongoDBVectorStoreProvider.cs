using AIKit.Core.VectorStores;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosMongoDB;
using MongoDB.Driver;

namespace AIKit.VectorStores.CosmosMongoDB;

public sealed class CosmosMongoDBVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "cosmos-mongodb";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "CosmosMongoDBVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.DatabaseName);

        var mongoClient = CreateMongoClient(settings);
        var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName);

        return new CosmosMongoVectorStore(mongoDatabase, new CosmosMongoVectorStoreOptions()
        {
            EmbeddingGenerator = settings.EmbeddingGenerator ?? throw new InvalidOperationException("An IEmbeddingGenerator must be provided in VectorStoreSettings."),
        });
    }

    private static MongoClient CreateMongoClient(VectorStoreSettings settings)
    {
        var connectionString = settings.ConnectionString!;

        // If any MongoDB-specific settings are provided, use MongoClientSettings
        if (settings.MongoConnectionTimeoutSeconds.HasValue ||
            settings.MongoMaxConnectionPoolSize.HasValue ||
            settings.MongoUseSsl.HasValue)
        {
            var mongoSettings = MongoClientSettings.FromConnectionString(connectionString);

            if (settings.MongoConnectionTimeoutSeconds.HasValue)
            {
                mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(settings.MongoConnectionTimeoutSeconds.Value);
            }

            if (settings.MongoMaxConnectionPoolSize.HasValue)
            {
                mongoSettings.MaxConnectionPoolSize = settings.MongoMaxConnectionPoolSize.Value;
            }

            if (settings.MongoUseSsl.HasValue)
            {
                mongoSettings.UseTls = settings.MongoUseSsl.Value;
            }

            return new MongoClient(mongoSettings);
        }

        // Default: use connection string directly
        return new MongoClient(connectionString);
    }
}