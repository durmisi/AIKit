using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosMongoDB;
using MongoDB.Driver;

namespace AIKit.Core.Vector;

public sealed class CosmosMongoDBVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "cosmos-mongodb";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "CosmosMongoDBVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.DatabaseName);

        var credential = ResolveCredential(settings);

        var mongoClient = new MongoClient(settings.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName ?? "aikit_vector_store");

        return new CosmosMongoVectorStore(mongoDatabase);
    }

    private static TokenCredential ResolveCredential(VectorStoreSettings settings)
    {
        return new DefaultAzureCredential();
    }
}