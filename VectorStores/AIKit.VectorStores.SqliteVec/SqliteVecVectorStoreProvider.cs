using AIKit.Core.VectorStores;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AIKit.VectorStores.SqliteVec;

public sealed class SqliteVecVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "sqlite-vec";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "SqliteVecVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        var options = new SqliteVectorStoreOptions
        {
            VectorVirtualTableName = settings.TableName,
            EmbeddingGenerator = settings.EmbeddingGenerator ?? throw new InvalidOperationException(
                "An IEmbeddingGenerator must be provided in VectorStoreSettings for SqliteVecVectorStoreProvider."),
        };

        return new SqliteVectorStore(settings.ConnectionString, options);
    }
}