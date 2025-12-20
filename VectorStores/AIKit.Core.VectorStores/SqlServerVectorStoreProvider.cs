using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqlServer;

namespace AIKit.Core.Vector;

public sealed class SqlServerVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "sql-server";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "SqlServerVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        return new SqlServerVectorStore(settings.ConnectionString);
    }
}