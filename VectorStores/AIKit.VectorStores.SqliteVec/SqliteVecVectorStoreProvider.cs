using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AIKit.VectorStores.SqliteVec;

public sealed class SqliteVecVectorStoreOptionsConfig
{
    public string ConnectionString { get; init; }
    public string? TableName { get; init; }
}

public sealed class VectorStoreBuilder
{
    public string Provider => "sqlite-vec";

    private readonly SqliteVecVectorStoreOptionsConfig _config;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public VectorStoreBuilder(
        IOptions<SqliteVecVectorStoreOptionsConfig> config,
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
            throw new InvalidOperationException("SQLite connection string is not configured.");
        }

        var options = SqliteVecVectorStoreOptionsFactory.Create(_config, _embeddingGenerator);

        return new SqliteVectorStore(_config.ConnectionString, options);
    }
}

internal static class SqliteVecVectorStoreOptionsFactory
{
    public static SqliteVectorStoreOptions Create(
        SqliteVecVectorStoreOptionsConfig config,
        IEmbeddingGenerator embeddingGenerator)
    {
        return new SqliteVectorStoreOptions
        {
            VectorVirtualTableName = config.TableName ?? "vectors",
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}

public sealed class SqliteVecVectorStoreBuilder
{
    private string? _connectionString;
    private string? _tableName;
    private SqliteVectorStoreOptions? _options;

    public SqliteVecVectorStoreBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public SqliteVecVectorStoreBuilder WithTableName(string tableName)
    {
        _tableName = tableName;
        return this;
    }

    public SqliteVecVectorStoreBuilder WithOptions(SqliteVectorStoreOptions options)
    {
        _options = options;
        return this;
    }

    public VectorStore Build()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("ConnectionString must be set.");

        if (_options is null)
            throw new InvalidOperationException("SqliteVectorStoreOptions must be set.");

        return new SqliteVectorStore(_connectionString, _options);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteVecVectorStore(
        this IServiceCollection services,
        Action<SqliteVecVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<VectorStoreBuilder>();

        return services;
    }

    public static IServiceCollection AddSqliteVecVectorStore(
        this IServiceCollection services,
        SqliteVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new SqliteVecFactory(store);

        services.AddSingleton<VectorStoreBuilder>(factory);

        return services;
    }

    private sealed class SqliteVecFactory
    {
        private readonly SqliteVectorStore _store;

        public SqliteVecFactory(SqliteVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "sqlite-vec";

        public VectorStore Build()
        {
            return _store;
        }
    }
}