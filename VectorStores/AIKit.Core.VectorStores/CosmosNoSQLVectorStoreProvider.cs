using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;

namespace AIKit.Core.Vector;

public sealed class CosmosNoSQLVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "cosmos-nosql";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "CosmosNoSQLVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var credential = ResolveCredential(settings);

        var clientOptions = new CosmosClientOptions
        {
        };  

        return new CosmosNoSqlVectorStore(settings.ConnectionString, settings.DatabaseName, clientOptions);
    }

    private static TokenCredential ResolveCredential(VectorStoreSettings settings)
    {
        return new DefaultAzureCredential();
    }
}