using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using MongoDB.Driver;

namespace AIKit.VectorStores.MongoDB;

public sealed class VectorStoreBuilder
{
    public string Provider => "mongodb";

    private string? _connectionString;
    private string? _databaseName;
    private IEmbeddingGenerator? _embeddingGenerator;

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

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Build()
    {
        Validate();

        var mongoClient = new MongoClient(_connectionString!);
        var mongoDatabase = mongoClient.GetDatabase(_databaseName ?? "vectorstore");

        var options = CreateStoreOptions(_embeddingGenerator!);

        return new MongoVectorStore(mongoDatabase, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("MongoDB connection string is not configured.");
        }

        if (_embeddingGenerator is null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private MongoVectorStoreOptions? CreateStoreOptions(IEmbeddingGenerator embeddingGenerator)
    {
        return new MongoVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}

