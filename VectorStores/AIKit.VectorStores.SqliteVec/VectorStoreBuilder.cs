using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AIKit.VectorStores.SqliteVec;

public sealed class VectorStoreBuilder
{
    public string Provider => "sqlite-vec";

    private string? _connectionString;
    private string? _tableName;
    private IEmbeddingGenerator? _embeddingGenerator;

    public VectorStoreBuilder()
    {
    }

    public VectorStoreBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        return this;
    }

    public VectorStoreBuilder WithTableName(string tableName)
    {
        _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
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

        var options = CreateStoreOptions(_embeddingGenerator!);

        return new SqliteVectorStore(_connectionString!, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("SQLite connection string is not configured.");
        }

        if (_embeddingGenerator == null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private SqliteVectorStoreOptions? CreateStoreOptions(IEmbeddingGenerator embeddingGenerator)
    {
        return new SqliteVectorStoreOptions
        {
            VectorVirtualTableName = _tableName ?? "vectors",
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}
