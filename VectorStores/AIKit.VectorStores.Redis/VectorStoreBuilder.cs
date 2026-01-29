using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;

namespace AIKit.VectorStores.Redis;

public sealed class VectorStoreBuilder
{
    public string Provider => "redis";

    private string? _endpoint;
    private string? _password;
    private int? _database;
    private int? _connectTimeout;
    private int? _syncTimeout;
    private IEmbeddingGenerator? _embeddingGenerator;

    public VectorStoreBuilder()
    {
    }

    public VectorStoreBuilder WithEndpoint(string endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        return this;
    }

    public VectorStoreBuilder WithPassword(string password)
    {
        _password = password ?? throw new ArgumentNullException(nameof(password));
        return this;
    }

    public VectorStoreBuilder WithDatabase(int database)
    {
        _database = database;
        return this;
    }

    public VectorStoreBuilder WithConnectTimeout(int connectTimeout)
    {
        _connectTimeout = connectTimeout;
        return this;
    }

    public VectorStoreBuilder WithSyncTimeout(int syncTimeout)
    {
        _syncTimeout = syncTimeout;
        return this;
    }

    public VectorStoreBuilder WithEmbeddingGenerator(IEmbeddingGenerator embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        return this;
    }

    public VectorStore Build()
    {
        Validate();

        var configuration = new ConfigurationOptions
        {
            EndPoints = { _endpoint! }
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

        var options = CreateStoreOptions(_embeddingGenerator!);

        return new RedisVectorStore(db, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
        {
            throw new InvalidOperationException("Redis endpoint is not configured.");
        }

        if (_embeddingGenerator == null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private static RedisVectorStoreOptions CreateStoreOptions(IEmbeddingGenerator embeddingGenerator)
    {
        return new RedisVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}

