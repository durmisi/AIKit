using Azure.Core;
using Azure.Identity;
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

        var clientOptions = new Microsoft.Azure.Cosmos.CosmosClientOptions
        {
            SerializerOptions = new Microsoft.Azure.Cosmos.CosmosSerializationOptions
            {
                PropertyNamingPolicy = Microsoft.Azure.Cosmos.CosmosPropertyNamingPolicy.CamelCase
            }
        };
        var storeOptions = new CosmosNoSqlVectorStoreOptions
        {
            EmbeddingGenerator = settings.EmbeddingGenerator ?? throw new InvalidOperationException("An IEmbeddingGenerator must be provided in VectorStoreSettings for CosmosNoSQLVectorStore."),
            JsonSerializerOptions = settings.JsonSerializerOptions
        };

        return new CosmosNoSqlVectorStore(settings.ConnectionString!, settings.DatabaseName!, clientOptions, storeOptions);
    }

    private static TokenCredential ResolveCredential(VectorStoreSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.ClientId) && !string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            return new ClientSecretCredential(
                settings.TenantId ?? throw new InvalidOperationException("TenantId must be provided when using ClientId and ClientSecret."),
                settings.ClientId,
                settings.ClientSecret);
        }

        return new DefaultAzureCredential();
    }

}