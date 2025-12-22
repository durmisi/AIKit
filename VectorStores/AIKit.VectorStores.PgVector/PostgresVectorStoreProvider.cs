using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;

namespace AIKit.VectorStores.PgVector;

public sealed class PostgresVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "postgres-vector";

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
}