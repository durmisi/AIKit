using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using MongoDB.Driver;

namespace AIKit.VectorStores.MongoDB;

public sealed class MongoDBVectorStoreOptionsConfig
{
    public string ConnectionString { get; init; }
    public string? DatabaseName { get; init; }
}

public sealed class MongoDBVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "mongodb";

    private readonly MongoDBVectorStoreOptionsConfig _config;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public MongoDBVectorStoreFactory(
        IOptions<MongoDBVectorStoreOptionsConfig> config,
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
        if (string.IsNullOrWhiteSpace(_config.ConnectionString))
        {
            throw new InvalidOperationException("MongoDB connection string is not configured.");
        }

        var mongoClient = new MongoClient(_config.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(_config.DatabaseName ?? "vectorstore");

        var options = MongoDBVectorStoreOptionsFactory.Create(_embeddingGenerator);

        return new MongoVectorStore(mongoDatabase, options);
    }
}

internal static class MongoDBVectorStoreOptionsFactory
{
    public static MongoVectorStoreOptions Create(IEmbeddingGenerator embeddingGenerator)
    {
        return new MongoVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}

public sealed class MongoDBVectorStoreBuilder
{
    private string? _connectionString;
    private string? _databaseName;
    private MongoVectorStoreOptions? _options;

    public MongoDBVectorStoreBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public MongoDBVectorStoreBuilder WithDatabaseName(string databaseName)
    {
        _databaseName = databaseName;
        return this;
    }

    public MongoDBVectorStoreBuilder WithOptions(MongoVectorStoreOptions options)
    {
        _options = options;
        return this;
    }

    public VectorStore Build()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("ConnectionString must be set.");

        if (_options is null)
            throw new InvalidOperationException("MongoVectorStoreOptions must be set.");

        var mongoClient = new MongoClient(_connectionString);
        var mongoDatabase = mongoClient.GetDatabase(_databaseName ?? "vectorstore");

        return new MongoVectorStore(mongoDatabase, _options);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDBVectorStore(
        this IServiceCollection services,
        Action<MongoDBVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<IVectorStoreFactory, MongoDBVectorStoreFactory>();

        return services;
    }

    public static IServiceCollection AddMongoDBVectorStore(
        this IServiceCollection services,
        MongoVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new MongoDBFactory(store);

        services.AddSingleton<IVectorStoreFactory>(factory);

        return services;
    }

    private sealed class MongoDBFactory : IVectorStoreFactory
    {
        private readonly MongoVectorStore _store;

        public MongoDBFactory(MongoVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "mongodb";

        public VectorStore Create()
        {
            return _store;
        }
    }
}