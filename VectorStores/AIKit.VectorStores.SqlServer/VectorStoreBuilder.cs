using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqlServer;

namespace AIKit.VectorStores.SqlServer;

public sealed class VectorStoreBuilder
{
    public string Provider => "sql-server";

    private string? _connectionString;
    private string? _schema;
    private IEmbeddingGenerator? _embeddingGenerator;

    public VectorStoreBuilder()
    {
    }

    public VectorStoreBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        return this;
    }

    public VectorStoreBuilder WithSchema(string schema)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
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

        var options = CreateOptions(_embeddingGenerator!);

        return new SqlServerVectorStore(_connectionString!, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("SQL Server connection string is not configured.");
        }

        if (_embeddingGenerator == null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private SqlServerVectorStoreOptions CreateOptions(IEmbeddingGenerator embeddingGenerator)
    {
        return new SqlServerVectorStoreOptions
        {
            Schema = _schema ?? "dbo",
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}
