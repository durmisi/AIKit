using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AIKit.VectorStores.SqliteVec;

public sealed class SqliteVecVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "sqlite-vec";

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
}