using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;

namespace AIKit.VectorStores.CosmosNoSQL;

public sealed class CosmosNoSQLVectorStoreOptionsConfig
{
    public string ConnectionString { get; init; }
    public string DatabaseName { get; init; }
}

public sealed class VectorStoreBuilder
{
    public string Provider => "cosmos-nosql";

    private readonly CosmosNoSQLVectorStoreOptionsConfig _config;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public VectorStoreBuilder(
        IOptions<CosmosNoSQLVectorStoreOptionsConfig> config,
        IEmbeddingGenerator embeddingGenerator)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
    }

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Build()
    {
        if (string.IsNullOrWhiteSpace(_config.ConnectionString))
        {
            throw new InvalidOperationException("Cosmos NoSQL connection string is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_config.DatabaseName))
        {
            throw new InvalidOperationException("Cosmos NoSQL database name is not configured.");
        }

        var clientOptions = new Microsoft.Azure.Cosmos.CosmosClientOptions
        {
            SerializerOptions = new Microsoft.Azure.Cosmos.CosmosSerializationOptions
            {
                PropertyNamingPolicy = Microsoft.Azure.Cosmos.CosmosPropertyNamingPolicy.CamelCase
            }
        };

        var storeOptions = CosmosNoSQLVectorStoreOptionsFactory.Create(_embeddingGenerator);

        return new CosmosNoSqlVectorStore(_config.ConnectionString, _config.DatabaseName, clientOptions, storeOptions);
    }
}

internal static class CosmosNoSQLVectorStoreOptionsFactory
{
    public static CosmosNoSqlVectorStoreOptions Create(IEmbeddingGenerator embeddingGenerator)
    {
        return new CosmosNoSqlVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            JsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = System.Diagnostics.Debugger.IsAttached,
            }
        };
    }
}

public sealed class CosmosNoSQLVectorStoreBuilder
{
    private string? _connectionString;
    private string? _databaseName;
    private Microsoft.Azure.Cosmos.CosmosClientOptions? _clientOptions;
    private CosmosNoSqlVectorStoreOptions? _storeOptions;

    public CosmosNoSQLVectorStoreBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public CosmosNoSQLVectorStoreBuilder WithDatabaseName(string databaseName)
    {
        _databaseName = databaseName;
        return this;
    }

    public CosmosNoSQLVectorStoreBuilder WithClientOptions(Microsoft.Azure.Cosmos.CosmosClientOptions clientOptions)
    {
        _clientOptions = clientOptions;
        return this;
    }

    public CosmosNoSQLVectorStoreBuilder WithStoreOptions(CosmosNoSqlVectorStoreOptions storeOptions)
    {
        _storeOptions = storeOptions;
        return this;
    }

    public VectorStore Build()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("ConnectionString must be set.");

        if (string.IsNullOrWhiteSpace(_databaseName))
            throw new InvalidOperationException("DatabaseName must be set.");

        if (_storeOptions is null)
            throw new InvalidOperationException("CosmosNoSqlVectorStoreOptions must be set.");

        var clientOptions = _clientOptions ?? new Microsoft.Azure.Cosmos.CosmosClientOptions
        {
            SerializerOptions = new Microsoft.Azure.Cosmos.CosmosSerializationOptions
            {
                PropertyNamingPolicy = Microsoft.Azure.Cosmos.CosmosPropertyNamingPolicy.CamelCase
            }
        };

        return new CosmosNoSqlVectorStore(_connectionString, _databaseName, clientOptions, _storeOptions);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosNoSQLVectorStore(
        this IServiceCollection services,
        Action<CosmosNoSQLVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<VectorStoreBuilder>();

        return services;
    }

    public static IServiceCollection AddCosmosNoSQLVectorStore(
        this IServiceCollection services,
        CosmosNoSqlVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new CosmosNoSQLFactory(store);

        services.AddSingleton<VectorStoreBuilder>(factory);

        return services;
    }

    private sealed class CosmosNoSQLFactory
    {
        private readonly CosmosNoSqlVectorStore _store;

        public CosmosNoSQLFactory(CosmosNoSqlVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "cosmos-nosql";

        public VectorStore Build()
        {
            return _store;
        }
    }
}