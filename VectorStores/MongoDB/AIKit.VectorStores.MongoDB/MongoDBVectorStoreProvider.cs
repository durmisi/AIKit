using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using MongoDB.Driver;

namespace AIKit.Core.Vector;

public sealed class MongoDBVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "mongodb";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "MongoDBVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        var mongoClient = new MongoClient(settings.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.DatabaseName ?? "vectorstore");

        var options = new MongoVectorStoreOptions
        {
            EmbeddingGenerator = settings.EmbeddingGenerator ?? throw new ArgumentNullException(nameof(settings.EmbeddingGenerator)),
        };

        return new MongoVectorStore(mongoDatabase, options);
    }
}