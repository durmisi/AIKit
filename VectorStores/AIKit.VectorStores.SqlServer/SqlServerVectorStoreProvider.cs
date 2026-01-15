using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqlServer;

namespace AIKit.VectorStores.SqlServer;

public sealed class SqlServerVectorStoreOptionsConfig
{
    public required string ConnectionString { get; init; }
    public string? Schema { get; init; }
}

public sealed class SqlServerVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "sql-server";

    private readonly SqlServerVectorStoreOptionsConfig _config;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public SqlServerVectorStoreFactory(
        IOptions<SqlServerVectorStoreOptionsConfig> config,
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
            throw new InvalidOperationException("SQL Server connection string is not configured.");
        }

        var options = SqlServerVectorStoreOptionsFactory.Create(_config, _embeddingGenerator);

        return new SqlServerVectorStore(_config.ConnectionString, options);
    }
}

internal static class SqlServerVectorStoreOptionsFactory
{
    public static SqlServerVectorStoreOptions Create(
        SqlServerVectorStoreOptionsConfig config,
        IEmbeddingGenerator embeddingGenerator)
    {
        return new SqlServerVectorStoreOptions
        {
            Schema = config.Schema ?? "dbo",
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}

public sealed class SqlServerVectorStoreBuilder
{
    private string? _connectionString;
    private SqlServerVectorStoreOptions? _options;

    public SqlServerVectorStoreBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public SqlServerVectorStoreBuilder WithOptions(SqlServerVectorStoreOptions options)
    {
        _options = options;
        return this;
    }

    public VectorStore Build()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("ConnectionString must be set.");

        if (_options is null)
            throw new InvalidOperationException("SqlServerVectorStoreOptions must be set.");

        return new SqlServerVectorStore(_connectionString, _options);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerVectorStore(
        this IServiceCollection services,
        Action<SqlServerVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<IVectorStoreFactory, SqlServerVectorStoreFactory>();

        return services;
    }

    public static IServiceCollection AddSqlServerVectorStore(
        this IServiceCollection services,
        SqlServerVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new SqlServerFactory(store);

        services.AddSingleton<IVectorStoreFactory>(factory);

        return services;
    }

    private sealed class SqlServerFactory : IVectorStoreFactory
    {
        private readonly SqlServerVectorStore _store;

        public SqlServerFactory(SqlServerVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "sql-server";

        public VectorStore Create()
        {
            return _store;
        }
    }
}