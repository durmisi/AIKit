using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;

namespace AIKit.VectorStores.Redis;

public sealed class RedisVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "redis";

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var db = CreateRedisDatabase(settings);

        var options = new RedisVectorStoreOptions
        {
            EmbeddingGenerator = VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings),
        };

        return new RedisVectorStore(db, options);
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using endpoint and embedding generator.
    /// Uses default Redis configuration.
    /// </summary>
    public VectorStore Create(string endpoint, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var configuration = new ConfigurationOptions
        {
            EndPoints = { endpoint }
        };

        var connection = ConnectionMultiplexer.Connect(configuration);
        var db = connection.GetDatabase();

        var options = new RedisVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };

        return new RedisVectorStore(db, options);
    }

    /// <summary>
    /// Creates a vector store with endpoint, password, and embedding generator.
    /// </summary>
    public VectorStore Create(string endpoint, string password, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var configuration = new ConfigurationOptions
        {
            EndPoints = { endpoint },
            Password = password
        };

        var connection = ConnectionMultiplexer.Connect(configuration);
        var db = connection.GetDatabase();

        var options = new RedisVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };

        return new RedisVectorStore(db, options);
    }

    /// <summary>
    /// Creates a vector store with a pre-configured Redis database and options.
    /// For advanced scenarios where full control over the Redis connection is needed.
    /// </summary>
    public VectorStore Create(IDatabase database, RedisVectorStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(options);

        return new RedisVectorStore(database, options);
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
}