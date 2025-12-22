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

    /// <summary>
    /// Creates a vector store with default settings (requires VectorStoreSettings).
    /// </summary>
    public VectorStore Create()
        => throw new InvalidOperationException(
            "AzureAISearchVectorStoreProvider requires VectorStoreSettings");

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

    private static Azure.Search.Documents.SearchClientOptions ResolveClientOptions(VectorStoreSettings settings)
    {
        var options = new Azure.Search.Documents.SearchClientOptions();

        // Apply additional settings for client configuration
        if (settings.AdditionalSettings != null)
        {
            if (settings.AdditionalSettings.TryGetValue("RetryDelay", out var retryDelay) && retryDelay is TimeSpan delay)
            {
                options.Retry.Delay = delay;
            }

            if (settings.AdditionalSettings.TryGetValue("RetryMaxRetries", out var maxRetries) && maxRetries is int retries)
            {
                options.Retry.MaxRetries = retries;
            }

            if (settings.AdditionalSettings.TryGetValue("RequestTimeout", out var timeout) && timeout is TimeSpan timeSpan)
            {
                options.Retry.NetworkTimeout = timeSpan;
            }
        }

        return options;
    }

    private static IEmbeddingGenerator? ResolveEmbeddingGenerator(VectorStoreSettings settings)
    {
        return settings.EmbeddingGenerator ?? throw new InvalidOperationException(
            "An IEmbeddingGenerator must be provided in VectorStoreSettings for AzureAISearchVectorStoreProvider.");
    }

    private static System.Text.Json.JsonSerializerOptions? ResolveJsonSerializerOptions(VectorStoreSettings settings)
    {
        return settings.JsonSerializerOptions ?? new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = Debugger.IsAttached,
        };
    }
}