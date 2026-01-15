using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;

namespace AIKit.VectorStores.PgVector;

public sealed class PostgresVectorStoreOptionsConfig
{
    public required string ConnectionString { get; init; }
    public string? Schema { get; init; }
}

public sealed class PostgresVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "postgres-vector";

    private readonly PostgresVectorStoreOptionsConfig _config;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public PostgresVectorStoreFactory(
        IOptions<PostgresVectorStoreOptionsConfig> config,
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
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        var options = PostgresVectorStoreOptionsFactory.Create(_config, _embeddingGenerator);

        return new PostgresVectorStore(_config.ConnectionString, options);
    }
}

internal static class PostgresVectorStoreOptionsFactory
{
    public static PostgresVectorStoreOptions Create(
        PostgresVectorStoreOptionsConfig config,
        IEmbeddingGenerator embeddingGenerator)
    {
        return new PostgresVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            Schema = config.Schema ?? "public",
        };
    }
}

public sealed class PostgresVectorStoreBuilder
{
    private string? _connectionString;
    private PostgresVectorStoreOptions? _options;

    public PostgresVectorStoreBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    public PostgresVectorStoreBuilder WithOptions(PostgresVectorStoreOptions options)
    {
        _options = options;
        return this;
    }

    public VectorStore Build()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
            throw new InvalidOperationException("ConnectionString must be set.");

        if (_options is null)
            throw new InvalidOperationException("PostgresVectorStoreOptions must be set.");

        return new PostgresVectorStore(_connectionString, _options);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresVectorStore(
        this IServiceCollection services,
        Action<PostgresVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<IVectorStoreFactory, PostgresVectorStoreFactory>();

        return services;
    }

    public static IServiceCollection AddPostgresVectorStore(
        this IServiceCollection services,
        PostgresVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new PostgresFactory(store);

        services.AddSingleton<IVectorStoreFactory>(factory);

        return services;
    }

    private sealed class PostgresFactory : IVectorStoreFactory
    {
        private readonly PostgresVectorStore _store;

        public PostgresFactory(PostgresVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "postgres-vector";

        public VectorStore Create()
        {
            return _store;
        }
    }
}