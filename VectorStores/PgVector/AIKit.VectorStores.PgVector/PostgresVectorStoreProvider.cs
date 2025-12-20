using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;

namespace AIKit.Core.Vector;

public sealed class PostgresVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "postgres-vector";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "PostgresVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        var options = new PostgresVectorStoreOptions
        {
            EmbeddingGenerator = settings.EmbeddingGenerator ?? throw new InvalidOperationException("An IEmbeddingGenerator must be provided in VectorStoreSettings for PostgresVectorStoreProvider."),
            Schema = settings.Schema ?? "public",
        };

        return new PostgresVectorStore(settings.ConnectionString, options);
    }
}