using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace AIKit.Core.Vector;

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

        return new SqliteVectorStore(settings.ConnectionString);
    }
}