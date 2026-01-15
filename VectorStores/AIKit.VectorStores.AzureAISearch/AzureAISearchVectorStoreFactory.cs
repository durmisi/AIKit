using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using System.Diagnostics;

namespace AIKit.VectorStores.AzureAISearch;

public sealed class AzureAISearchVectorStoreOptionsConfig
{
    public required Uri Endpoint { get; init; }
}

public sealed class AzureAISearchVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "azure-ai-search";

    private readonly AzureAISearchVectorStoreOptionsConfig _config;
    private readonly TokenCredential _credential;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public AzureAISearchVectorStoreFactory(
        IOptions<AzureAISearchVectorStoreOptionsConfig> config,
        TokenCredential credential,
        IEmbeddingGenerator embeddingGenerator)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
    }

    public VectorStore Create()
    {
        if (string.IsNullOrWhiteSpace(_config.Endpoint?.ToString()))
        {
            throw new InvalidOperationException("Azure AI Search endpoint is not configured.");
        }

        var client = new SearchIndexClient(_config.Endpoint, _credential);

        var options = AzureAISearchVectorStoreOptionsFactory.Create(_embeddingGenerator);

        return new AzureAISearchVectorStore(client, options);
    }
}

internal static class AzureAISearchVectorStoreOptionsFactory
{
    public static AzureAISearchVectorStoreOptions Create(
        IEmbeddingGenerator embeddingGenerator,
        Action<AzureAISearchVectorStoreOptions>? configure = null)
    {
        var options = new AzureAISearchVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
            JsonSerializerOptions = new()
            {
                WriteIndented = Debugger.IsAttached
            }
        };

        configure?.Invoke(options);
        return options;
    }
}

public sealed class AzureAISearchVectorStoreBuilder
{
    private SearchIndexClient? _client;
    private AzureAISearchVectorStoreOptions? _options;

    public AzureAISearchVectorStoreBuilder WithClient(SearchIndexClient client)
    {
        _client = client;
        return this;
    }

    public AzureAISearchVectorStoreBuilder WithOptions(AzureAISearchVectorStoreOptions options)
    {
        _options = options;
        return this;
    }

    public VectorStore Build()
    {
        if (_client is null)
            throw new InvalidOperationException("SearchIndexClient must be set.");

        if (_options is null)
            throw new InvalidOperationException("AzureAISearchVectorStoreOptions must be set.");

        return new AzureAISearchVectorStore(_client, _options);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAISearchVectorStore(
    this IServiceCollection services,
    Action<AzureAISearchVectorStoreOptionsConfig> configure,
    TokenCredential? credential = null)
    {
        // Configure endpoint
        services.Configure(configure);

        // Register credential
        if (credential != null)
        {
            services.AddSingleton(credential);
        }
        else
        {
            services.AddSingleton<TokenCredential, DefaultAzureCredential>();
        }

        // Register the factory
        services.AddSingleton<IVectorStoreFactory, AzureAISearchVectorStoreFactory>();

        return services;
    }


    public static IServiceCollection AddAzureAISearchVectorStore(
           this IServiceCollection services,
           AzureAISearchVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new AzureAISearchFactory(store);

        services.AddSingleton<IVectorStoreFactory>(factory);

        return services;
    }

    private sealed class AzureAISearchFactory : IVectorStoreFactory
    {
        private readonly AzureAISearchVectorStore _store;

        public AzureAISearchFactory(AzureAISearchVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "azure-ai-search";

        public VectorStore Create()
        {
            return _store;
        }
    }
}

