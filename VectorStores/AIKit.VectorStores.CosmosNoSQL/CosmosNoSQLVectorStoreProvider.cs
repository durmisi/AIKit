using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Azure.Core;
using Microsoft.Extensions.AI;

namespace AIKit.VectorStores.CosmosNoSQL;

public sealed class CosmosNoSQLVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "cosmos-nosql";

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.ConnectionString);

        var credential = VectorStoreProviderHelpers.ResolveAzureCredential(settings);

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

    /// <summary>
    /// Creates a vector store with minimal configuration using connection string, database name, and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, string databaseName, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var clientOptions = new Microsoft.Azure.Cosmos.CosmosClientOptions
        {
            SerializerOptions = new Microsoft.Azure.Cosmos.CosmosSerializationOptions
            {
                PropertyNamingPolicy = Microsoft.Azure.Cosmos.CosmosPropertyNamingPolicy.CamelCase
            }
        };
        var storeOptions = new CosmosNoSqlVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            JsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = System.Diagnostics.Debugger.IsAttached,
            }
        };

        return new CosmosNoSqlVectorStore(connectionString, databaseName, clientOptions, storeOptions);
    }

    /// <summary>
    /// Creates a vector store with connection string, database name, credential, and embedding generator.
    /// </summary>
    public VectorStore Create(string connectionString, string databaseName, TokenCredential credential, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentNullException.ThrowIfNull(credential);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var clientOptions = new Microsoft.Azure.Cosmos.CosmosClientOptions
        {
            SerializerOptions = new Microsoft.Azure.Cosmos.CosmosSerializationOptions
            {
                PropertyNamingPolicy = Microsoft.Azure.Cosmos.CosmosPropertyNamingPolicy.CamelCase
            }
        };
        var storeOptions = new CosmosNoSqlVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            JsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = System.Diagnostics.Debugger.IsAttached,
            }
        };

        return new CosmosNoSqlVectorStore(connectionString, databaseName, clientOptions, storeOptions);
    }
}