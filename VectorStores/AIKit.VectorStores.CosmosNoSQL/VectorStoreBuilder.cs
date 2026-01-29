using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;

namespace AIKit.VectorStores.CosmosNoSQL;


public class VectorStoreBuilder
{
    public string Provider => "cosmos-nosql";

    private string? _connectionString;
    private string? _databaseName;
    private IEmbeddingGenerator? _embeddingGenerator;
    private Microsoft.Azure.Cosmos.CosmosClientOptions? _clientOptions;
    private CosmosNoSqlVectorStoreOptions? _storeOptions;
    protected VectorStore? _store;

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

    public VectorStoreBuilder WithClientOptions(Microsoft.Azure.Cosmos.CosmosClientOptions clientOptions)
    {
        _clientOptions = clientOptions ?? throw new ArgumentNullException(nameof(clientOptions));
        return this;
    }

    public VectorStoreBuilder WithStoreOptions(CosmosNoSqlVectorStoreOptions storeOptions)
    {
        _storeOptions = storeOptions ?? throw new ArgumentNullException(nameof(storeOptions));
        return this;
    }

    public virtual VectorStore Build()
    {
        if (_store != null) return _store;

        Validate();

        var clientOptions = _clientOptions ?? CreateClientOptions();

        var storeOptions = _storeOptions ?? CreateStoreOptions(_embeddingGenerator!);

        return new CosmosNoSqlVectorStore(_connectionString!, _databaseName!, clientOptions, storeOptions);
    }

    private CosmosClientOptions? CreateClientOptions()
    {
        return new Microsoft.Azure.Cosmos.CosmosClientOptions
        {
            SerializerOptions = new Microsoft.Azure.Cosmos.CosmosSerializationOptions
            {
                PropertyNamingPolicy = Microsoft.Azure.Cosmos.CosmosPropertyNamingPolicy.CamelCase
            }
        };
    }

    static CosmosNoSqlVectorStoreOptions CreateStoreOptions(IEmbeddingGenerator embeddingGenerator)
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

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("Cosmos NoSQL connection string is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_databaseName))
        {
            throw new InvalidOperationException("Cosmos NoSQL database name is not configured.");
        }

        if (_embeddingGenerator == null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }
}