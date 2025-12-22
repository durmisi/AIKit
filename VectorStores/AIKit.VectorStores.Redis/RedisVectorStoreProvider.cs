using AIKit.Core.VectorStores;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;

namespace AIKit.VectorStores.Redis;

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

        var db = CreateRedisDatabase(settings);

        var options = new RedisVectorStoreOptions
        {
            EmbeddingGenerator = ResolveEmbeddingGenerator(settings),
        };

        return new RedisVectorStore(db, options);
    }

    private static IDatabase CreateRedisDatabase(VectorStoreSettings settings)
    {
        var configuration = new ConfigurationOptions
        {
            EndPoints = { settings.Endpoint }
        };

        // Apply additional settings
        if (settings.AdditionalSettings != null)
        {
            if (settings.AdditionalSettings.TryGetValue("Password", out var password) && password is string pwd)
            {
                configuration.Password = pwd;
            }

            if (settings.AdditionalSettings.TryGetValue("Database", out var database) && database is int dbNum)
            {
                configuration.DefaultDatabase = dbNum;
            }

            if (settings.AdditionalSettings.TryGetValue("ConnectTimeout", out var timeout) && timeout is int timeoutMs)
            {
                configuration.ConnectTimeout = timeoutMs;
            }

            if (settings.AdditionalSettings.TryGetValue("SyncTimeout", out var syncTimeout) && syncTimeout is int syncTimeoutMs)
            {
                configuration.SyncTimeout = syncTimeoutMs;
            }

            // Add more as needed
        }

        var connection = ConnectionMultiplexer.Connect(configuration);
        return connection.GetDatabase();
    }

    private static IEmbeddingGenerator? ResolveEmbeddingGenerator(
        VectorStoreSettings settings)
    {
        return settings.EmbeddingGenerator ?? throw new InvalidOperationException(
            "An IEmbeddingGenerator must be provided in VectorStoreSettings for RedisVectorStoreProvider.");
    }
}