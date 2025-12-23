using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using Microsoft.Extensions.AI;

namespace AIKit.VectorStores.SqliteVec;

public sealed class SqliteVecVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "sqlite-vec";

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        var options = new SqliteVectorStoreOptions
        {
            VectorVirtualTableName = settings.TableName,
            EmbeddingGenerator = VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings),
        };

        return new SqliteVectorStore(settings.ConnectionString, options);
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using connection string and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var options = new SqliteVectorStoreOptions
        {
            VectorVirtualTableName = "vectors",
            EmbeddingGenerator = embeddingGenerator,
        };

        return new SqliteVectorStore(connectionString, options);
    }

    /// <summary>
    /// Creates a vector store with connection string, table name, and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, string tableName, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var options = new SqliteVectorStoreOptions
        {
            VectorVirtualTableName = tableName,
            EmbeddingGenerator = embeddingGenerator,
        };

        return new SqliteVectorStore(connectionString, options);
    }
}