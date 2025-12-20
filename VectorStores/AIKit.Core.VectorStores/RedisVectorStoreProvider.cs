using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;

namespace AIKit.Core.Vector;

public sealed class RedisVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "redis";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "RedisVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var connection = ConnectionMultiplexer.Connect(settings.Endpoint);
        var db = connection.GetDatabase();

        var options = new RedisVectorStoreOptions
        {
            EmbeddingGenerator = ResolveEmbeddingGenerator(settings),
        };

        return new RedisVectorStore(db, options);
    }

    private static IEmbeddingGenerator? ResolveEmbeddingGenerator(
        VectorStoreSettings settings)
    {
        return settings.EmbeddingGenerator ?? throw new InvalidOperationException(
            "An IEmbeddingGenerator must be provided in VectorStoreSettings for RedisVectorStoreProvider.");
    }
}