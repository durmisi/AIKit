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

        var options = new SqlServerVectorStoreOptions
        {
            Schema = settings.Schema ?? "dbo",
            EmbeddingGenerator = settings.EmbeddingGenerator ?? throw new ArgumentNullException(nameof(settings.EmbeddingGenerator)),
        };

        return new SqlServerVectorStore(settings.ConnectionString, options);
    }
}