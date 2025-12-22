using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqlServer;

namespace AIKit.VectorStores.SqlServer;

public sealed class SqlServerVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "sql-server";

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
}