using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;

namespace AIKit.VectorStores.Redis;

public sealed class RedisVectorStoreOptionsConfig
{
    public required string Endpoint { get; init; }
    public string? Password { get; init; }
    public int? Database { get; init; }
    public int? ConnectTimeout { get; init; }
    public int? SyncTimeout { get; init; }
}

public sealed class RedisVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "redis";

    private readonly RedisVectorStoreOptionsConfig _config;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public RedisVectorStoreFactory(
        IOptions<RedisVectorStoreOptionsConfig> config,
        IEmbeddingGenerator embeddingGenerator)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
    }

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create()
    {
        if (string.IsNullOrWhiteSpace(_config.Endpoint))
        {
            throw new InvalidOperationException("Redis endpoint is not configured.");
        }

        var configuration = new ConfigurationOptions
        {
            EndPoints = { _config.Endpoint }
        };

        if (!string.IsNullOrEmpty(_config.Password))
        {
            configuration.Password = _config.Password;
        }

        if (_config.Database.HasValue)
        {
            configuration.DefaultDatabase = _config.Database.Value;
        }

        if (_config.ConnectTimeout.HasValue)
        {
            configuration.ConnectTimeout = _config.ConnectTimeout.Value;
        }

        if (_config.SyncTimeout.HasValue)
        {
            configuration.SyncTimeout = _config.SyncTimeout.Value;
        }

        var connection = ConnectionMultiplexer.Connect(configuration);
        var db = connection.GetDatabase();

        var options = RedisVectorStoreOptionsFactory.Create(_embeddingGenerator);

        return new RedisVectorStore(db, options);
    }
}

internal static class RedisVectorStoreOptionsFactory
{
    public static RedisVectorStoreOptions Create(IEmbeddingGenerator embeddingGenerator)
    {
        return new RedisVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}

public sealed class RedisVectorStoreBuilder
{
    private string? _endpoint;
    private string? _password;
    private int? _database;
    private int? _connectTimeout;
    private int? _syncTimeout;
    private RedisVectorStoreOptions? _options;

    public RedisVectorStoreBuilder WithEndpoint(string endpoint)
    {
        _endpoint = endpoint;
        return this;
    }

    public RedisVectorStoreBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public RedisVectorStoreBuilder WithDatabase(int database)
    {
        _database = database;
        return this;
    }

    public RedisVectorStoreBuilder WithConnectTimeout(int timeout)
    {
        _connectTimeout = timeout;
        return this;
    }

    public RedisVectorStoreBuilder WithSyncTimeout(int timeout)
    {
        _syncTimeout = timeout;
        return this;
    }

    public RedisVectorStoreBuilder WithOptions(RedisVectorStoreOptions options)
    {
        _options = options;
        return this;
    }

    public VectorStore Build()
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
            throw new InvalidOperationException("Endpoint must be set.");

        if (_options is null)
            throw new InvalidOperationException("RedisVectorStoreOptions must be set.");

        var configuration = new ConfigurationOptions
        {
            EndPoints = { _endpoint }
        };

        if (!string.IsNullOrEmpty(_password))
        {
            configuration.Password = _password;
        }

        if (_database.HasValue)
        {
            configuration.DefaultDatabase = _database.Value;
        }

        if (_connectTimeout.HasValue)
        {
            configuration.ConnectTimeout = _connectTimeout.Value;
        }

        if (_syncTimeout.HasValue)
        {
            configuration.SyncTimeout = _syncTimeout.Value;
        }

        var connection = ConnectionMultiplexer.Connect(configuration);
        var db = connection.GetDatabase();

        return new RedisVectorStore(db, _options);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisVectorStore(
        this IServiceCollection services,
        Action<RedisVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<IVectorStoreFactory, RedisVectorStoreFactory>();

        return services;
    }

    public static IServiceCollection AddRedisVectorStore(
        this IServiceCollection services,
        RedisVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new RedisFactory(store);

        services.AddSingleton<IVectorStoreFactory>(factory);

        return services;
    }

    private sealed class RedisFactory : IVectorStoreFactory
    {
        private readonly RedisVectorStore _store;

        public RedisFactory(RedisVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "redis";

        public VectorStore Create()
        {
            return _store;
        }
    }
}