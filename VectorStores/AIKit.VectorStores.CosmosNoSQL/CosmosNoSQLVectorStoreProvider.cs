using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Azure.Core;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;

namespace AIKit.VectorStores.CosmosNoSQL;

public sealed class CosmosNoSQLVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "cosmos-nosql";

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        var credential = ResolveCredential(settings);

        var clientOptions = new Microsoft.Azure.Cosmos.CosmosClientOptions
        {
            SerializerOptions = new Microsoft.Azure.Cosmos.CosmosSerializationOptions
            {
                PropertyNamingPolicy = Microsoft.Azure.Cosmos.CosmosPropertyNamingPolicy.CamelCase
            }
        };
        var storeOptions = new CosmosNoSqlVectorStoreOptions
        {
            EmbeddingGenerator = VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings),
            JsonSerializerOptions = VectorStoreProviderHelpers.ResolveJsonSerializerOptions(settings)
        };

        return new CosmosNoSqlVectorStore(settings.ConnectionString, settings.DatabaseName!, clientOptions, storeOptions);
    }

    private static TokenCredential ResolveCredential(VectorStoreSettings settings)
    {
        return VectorStoreProviderHelpers.ResolveAzureCredential(settings);
    }

}