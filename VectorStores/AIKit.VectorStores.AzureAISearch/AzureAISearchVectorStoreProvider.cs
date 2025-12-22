using AIKit.Core.VectorStores;
using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using System.Diagnostics;

namespace AIKit.VectorStores.AzureAISearch;

public sealed class AzureAISearchVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "azure-ai-search";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "AzureAISearchVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        // 1? Credential resolution
        var credential = ResolveCredential(settings);

        // 2? Native Azure SDK client
        var searchIndexClient = new SearchIndexClient(new Uri(settings.Endpoint), credential,
            options: new Azure.Search.Documents.SearchClientOptions());

        // 3? Vector store options (AI-specific concerns)
        var options = new AzureAISearchVectorStoreOptions
        {
            EmbeddingGenerator = ResolveEmbeddingGenerator(settings),
            JsonSerializerOptions = ResolveJsonSerializerOptions(settings),
        };

        return new AzureAISearchVectorStore(searchIndexClient, options);
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

    private static IEmbeddingGenerator? ResolveEmbeddingGenerator(
        VectorStoreSettings settings)
    {
        return settings.EmbeddingGenerator ?? throw new InvalidOperationException(
            "An IEmbeddingGenerator must be provided in VectorStoreSettings for AzureAISearchVectorStoreProvider.");
    }

    private static System.Text.Json.JsonSerializerOptions? ResolveJsonSerializerOptions(
        VectorStoreSettings settings)
    {
        return settings.JsonSerializerOptions ?? new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = Debugger.IsAttached,
        };
    }
}