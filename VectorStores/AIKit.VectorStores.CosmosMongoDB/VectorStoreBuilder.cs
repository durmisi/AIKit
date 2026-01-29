using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosMongoDB;
using MongoDB.Driver;

namespace AIKit.VectorStores.CosmosMongoDB;

public sealed class MongoConnectionOptions
{
    public int? ConnectionTimeoutSeconds { get; init; }
    public int? MaxConnectionPoolSize { get; init; }
    public bool? UseSsl { get; init; }
}

public sealed class VectorStoreBuilder
{
    public string Provider => "cosmos-mongodb";

    private string? _connectionString;
    private string? _databaseName;
    private IEmbeddingGenerator? _embeddingGenerator;
    private MongoConnectionOptions? _mongoConnectionOptions;

    public VectorStoreBuilder()
    {
    }

    public VectorStoreBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        return this;
    }

    public VectorStoreBuilder WithDatabaseName(string databaseName)
    {
        _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        return this;
    }

    public VectorStoreBuilder WithEmbeddingGenerator(IEmbeddingGenerator embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        return this;
    }

    public VectorStoreBuilder WithMongoConnectionOptions(MongoConnectionOptions options)
    {
        _mongoConnectionOptions = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    public VectorStore Build()
    {
        Validate();

        var mongoClient = CreateMongoClient();
        var mongoDatabase = mongoClient.GetDatabase(_databaseName!);

        var options = new CosmosMongoVectorStoreOptions
        {
            EmbeddingGenerator = _embeddingGenerator!,
        };

        return new CosmosMongoVectorStore(mongoDatabase, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("Cosmos MongoDB connection string is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_databaseName))
        {
            throw new InvalidOperationException("Cosmos MongoDB database name is not configured.");
        }

        if (_embeddingGenerator == null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private MongoClient CreateMongoClient()
    {
        var connectionString = _connectionString!;

        // If MongoDB-specific settings are provided, use MongoClientSettings
        if (_mongoConnectionOptions != null &&
            (_mongoConnectionOptions.ConnectionTimeoutSeconds.HasValue ||
             _mongoConnectionOptions.MaxConnectionPoolSize.HasValue ||
             _mongoConnectionOptions.UseSsl.HasValue))
        {
            var mongoSettings = MongoClientSettings.FromConnectionString(connectionString);

            if (_mongoConnectionOptions.ConnectionTimeoutSeconds.HasValue)
            {
                mongoSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(_mongoConnectionOptions.ConnectionTimeoutSeconds.Value);
            }

            if (_mongoConnectionOptions.MaxConnectionPoolSize.HasValue)
            {
                mongoSettings.MaxConnectionPoolSize = _mongoConnectionOptions.MaxConnectionPoolSize.Value;
            }

            if (_mongoConnectionOptions.UseSsl.HasValue)
            {
                mongoSettings.UseTls = _mongoConnectionOptions.UseSsl.Value;
            }

            return new MongoClient(mongoSettings);
        }

        // Default: use connection string directly
        return new MongoClient(connectionString);
    }
}