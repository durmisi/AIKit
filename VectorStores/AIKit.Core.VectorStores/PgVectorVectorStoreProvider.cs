using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;

namespace AIKit.Core.Vector;

public sealed class PgVectorVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "pg-vector";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "PgVectorVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        return new PostgresVectorStore(settings.ConnectionString!);
    }
}