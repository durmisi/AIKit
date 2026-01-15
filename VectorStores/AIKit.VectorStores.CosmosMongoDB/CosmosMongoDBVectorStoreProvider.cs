using AIKit.Core.VectorStores;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosMongoDB;
using MongoDB.Driver;

namespace AIKit.VectorStores.CosmosMongoDB;

public sealed class CosmosMongoDBVectorStoreOptionsConfig
{
    public required string ConnectionString { get; init; }
    public required string DatabaseName { get; init; }
    public int? MongoConnectionTimeoutSeconds { get; init; }
    public int? MongoMaxConnectionPoolSize { get; init; }
    public bool? MongoUseSsl { get; init; }
}

public sealed class CosmosMongoDBVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "cosmos-mongodb";

    private readonly CosmosMongoDBVectorStoreOptionsConfig _config;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public CosmosMongoDBVectorStoreFactory(
        IOptions<CosmosMongoDBVectorStoreOptionsConfig> config,
        IEmbeddingGenerator embeddingGenerator)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
    }

    public VectorStore Create()
    {
        if (string.IsNullOrWhiteSpace(_config.ConnectionString))
        {
            throw new InvalidOperationException("Cosmos MongoDB connection string is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_config.DatabaseName))
        {
            throw new InvalidOperationException("Cosmos MongoDB database name is not configured.");
        }

        var mongoClient = CreateMongoClient(_config);
        var mongoDatabase = mongoClient.GetDatabase(_config.DatabaseName);

        var options = CosmosMongoDBVectorStoreOptionsFactory.Create(_config, _embeddingGenerator);

        return new CosmosMongoVectorStore(mongoDatabase, options);
    }

    private static MongoClient CreateMongoClient(CosmosMongoDBVectorStoreOptionsConfig config)
    {
        var connectionString = config.ConnectionString;

        // If any MongoDB-specific settings are provided, use MongoClientSettings
        if (config.MongoConnectionTimeoutSeconds.HasValue ||
            config.MongoMaxConnectionPoolSize.HasValue ||
            config.MongoUseSsl.HasValue)
        {
            var mongoSettings = MongoClientSettings.FromConnectionString(connectionString);

            if (config.MongoConnectionTimeoutSeconds.HasValue)
            {
                mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(config.MongoConnectionTimeoutSeconds.Value);
            }

            if (config.MongoMaxConnectionPoolSize.HasValue)
            {
                mongoSettings.MaxConnectionPoolSize = config.MongoMaxConnectionPoolSize.Value;
            }

            if (config.MongoUseSsl.HasValue)
            {
                mongoSettings.UseTls = config.MongoUseSsl.Value;
            }

            return new MongoClient(mongoSettings);
        }

        // Default: use connection string directly
        return new MongoClient(connectionString);
    }
}

internal static class CosmosMongoDBVectorStoreOptionsFactory
{
    public static CosmosMongoVectorStoreOptions Create(
        CosmosMongoDBVectorStoreOptionsConfig config,
        IEmbeddingGenerator embeddingGenerator)
    {
        return new CosmosMongoVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}

public sealed class CosmosMongoDBVectorStoreBuilder
{
    private IMongoDatabase? _database;
    private CosmosMongoVectorStoreOptions? _options;

    public CosmosMongoDBVectorStoreBuilder WithDatabase(IMongoDatabase database)
    {
        _database = database;
        return this;
    }

    public CosmosMongoDBVectorStoreBuilder WithOptions(CosmosMongoVectorStoreOptions options)
    {
        _options = options;
        return this;
    }

    public VectorStore Build()
    {
        if (_database is null)
            throw new InvalidOperationException("IMongoDatabase must be set.");

        if (_options is null)
            throw new InvalidOperationException("CosmosMongoVectorStoreOptions must be set.");

        return new CosmosMongoVectorStore(_database, _options);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosMongoDBVectorStore(
        this IServiceCollection services,
        Action<CosmosMongoDBVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<IVectorStoreFactory, CosmosMongoDBVectorStoreFactory>();

        return services;
    }

    public static IServiceCollection AddCosmosMongoDBVectorStore(
        this IServiceCollection services,
        CosmosMongoVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new CosmosMongoDBFactory(store);

        services.AddSingleton<IVectorStoreFactory>(factory);

        return services;
    }

    private sealed class CosmosMongoDBFactory : IVectorStoreFactory
    {
        private readonly CosmosMongoVectorStore _store;

        public CosmosMongoDBFactory(CosmosMongoVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "cosmos-mongodb";

        public VectorStore Create()
        {
            return _store;
        }
    }
}