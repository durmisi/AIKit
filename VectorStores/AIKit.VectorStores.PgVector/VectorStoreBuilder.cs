using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;

namespace AIKit.VectorStores.PgVector;

public sealed class VectorStoreBuilder
{
    public string Provider => "postgres-vector";

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

    public VectorStoreBuilder WithSchema(string? schema)
    {
        _schema = schema;
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

        var options = CreateStoreOptions(_embeddingGenerator!, _schema);

        return new PostgresVectorStore(_connectionString!, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("PostgreSQL connection string is not configured.");
        }

        if (_embeddingGenerator == null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private static PostgresVectorStoreOptions CreateStoreOptions(IEmbeddingGenerator embeddingGenerator, string? schema)
    {
        return new PostgresVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            Schema = schema ?? "public",
        };
    }
}
