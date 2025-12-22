using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
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

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var credential = ResolveCredential(settings);
        var clientOptions = ResolveClientOptions(settings);
        var searchIndexClient = new SearchIndexClient(new Uri(settings.Endpoint), credential, clientOptions);

        var options = new AzureAISearchVectorStoreOptions
        {
            EmbeddingGenerator = ResolveEmbeddingGenerator(settings),
            JsonSerializerOptions = ResolveJsonSerializerOptions(settings),
        };

        return new AzureAISearchVectorStore(searchIndexClient, options);
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using endpoint and embedding generator.
    /// Uses DefaultAzureCredential for authentication.
    /// </summary>
    public VectorStore Create(string endpoint, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var credential = new DefaultAzureCredential();
        var searchIndexClient = new SearchIndexClient(new Uri(endpoint), credential);

        var options = new AzureAISearchVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            JsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = Debugger.IsAttached,
            },
        };

        return new AzureAISearchVectorStore(searchIndexClient, options);
    }

    /// <summary>
    /// Creates a vector store with endpoint, credential, and embedding generator.
    /// </summary>
    public VectorStore Create(string endpoint, TokenCredential credential, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(credential);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var searchIndexClient = new SearchIndexClient(new Uri(endpoint), credential);

        var options = new AzureAISearchVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            JsonSerializerOptions = new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = Debugger.IsAttached,
            },
        };

        return new AzureAISearchVectorStore(searchIndexClient, options);
    }

    /// <summary>
    /// Creates a vector store with endpoint, credential, embedding generator, and custom options.
    /// </summary>
    public VectorStore Create(string endpoint, TokenCredential credential, IEmbeddingGenerator embeddingGenerator, AzureAISearchVectorStoreOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(credential);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);
        ArgumentNullException.ThrowIfNull(options);

        var clientOptions = new Azure.Search.Documents.SearchClientOptions();
        var searchIndexClient = new SearchIndexClient(new Uri(endpoint), credential, clientOptions);

        return new AzureAISearchVectorStore(searchIndexClient, options);
    }

    /// <summary>
    /// Creates a vector store with a pre-configured SearchIndexClient and options.
    /// For advanced scenarios where full control over the client is needed.
    /// </summary>
    public VectorStore Create(SearchIndexClient searchIndexClient, AzureAISearchVectorStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(searchIndexClient);
        ArgumentNullException.ThrowIfNull(options);

        return new AzureAISearchVectorStore(searchIndexClient, options);
    }

    // REUSABLE PATTERNS - Can be copied to other providers

    /// <summary>
    /// Resolves Azure credentials from settings. Supports ClientSecretCredential and DefaultAzureCredential.
    /// Can be reused in CosmosNoSQL, CosmosMongoDB providers.
    /// </summary>
    public static TokenCredential ResolveCredential(VectorStoreSettings settings)
    {
        return VectorStoreProviderHelpers.ResolveAzureCredential(settings);
    }

    /// <summary>
    /// Resolves Azure Search client options from settings AdditionalSettings.
    /// Can be adapted for other Azure services (Cosmos, etc.).
    /// </summary>
    public static Azure.Search.Documents.SearchClientOptions ResolveClientOptions(VectorStoreSettings settings)
    {
        var options = new Azure.Search.Documents.SearchClientOptions();

        // Apply additional settings for client configuration
        var retryDelay = VectorStoreProviderHelpers.GetAdditionalSetting<TimeSpan>(settings, "RetryDelay");
        if (retryDelay != default)
        {
            options.Retry.Delay = retryDelay;
        }

        var maxRetries = VectorStoreProviderHelpers.GetAdditionalSetting<int>(settings, "RetryMaxRetries");
        if (maxRetries > 0)
        {
            options.Retry.MaxRetries = maxRetries;
        }

        var timeout = VectorStoreProviderHelpers.GetAdditionalSetting<TimeSpan>(settings, "RequestTimeout");
        if (timeout != default)
        {
            options.Retry.NetworkTimeout = timeout;
        }

        return options;
    }

    /// <summary>
    /// Ensures an embedding generator is provided. Can be reused in all vector store providers.
    /// </summary>
    public static IEmbeddingGenerator ResolveEmbeddingGenerator(VectorStoreSettings settings)
    {
        return VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings);
    }

    /// <summary>
    /// Resolves JSON serializer options with smart defaults. Can be reused in providers that support JSON options.
    /// </summary>
    public static System.Text.Json.JsonSerializerOptions ResolveJsonSerializerOptions(VectorStoreSettings settings)
    {
        return VectorStoreProviderHelpers.ResolveJsonSerializerOptions(settings);
    }
}