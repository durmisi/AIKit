using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Microsoft.Extensions.AI;

namespace AIKit.VectorStores.PgVector;

public sealed class PostgresVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "postgres-vector";

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        var options = new PostgresVectorStoreOptions
        {
            EmbeddingGenerator = VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings),
            Schema = settings.Schema ?? "public",
        };

        return new PostgresVectorStore(settings.ConnectionString, options);
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using connection string and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var options = new PostgresVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            Schema = "public",
        };

        return new PostgresVectorStore(connectionString, options);
    }

    /// <summary>
    /// Creates a vector store with connection string, schema, and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, string schema, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var options = new PostgresVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            Schema = schema,
        };

        return new PostgresVectorStore(connectionString, options);
    }
}