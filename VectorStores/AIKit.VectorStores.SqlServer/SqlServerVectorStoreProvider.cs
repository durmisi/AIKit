using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqlServer;

namespace AIKit.VectorStores.SqlServer;

public sealed class SqlServerVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "sql-server";

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        var options = new SqlServerVectorStoreOptions
        {
            Schema = settings.Schema ?? "dbo",
            EmbeddingGenerator = VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings),
        };

        return new SqlServerVectorStore(settings.ConnectionString, options);
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using connection string and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var options = new SqlServerVectorStoreOptions
        {
            Schema = "dbo",
            EmbeddingGenerator = embeddingGenerator,
        };

        return new SqlServerVectorStore(connectionString, options);
    }

    /// <summary>
    /// Creates a vector store with connection string, schema, and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, string schema, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var options = new SqlServerVectorStoreOptions
        {
            Schema = schema,
            EmbeddingGenerator = embeddingGenerator,
        };

        return new SqlServerVectorStore(connectionString, options);
    }
}