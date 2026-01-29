using Azure.Core;
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
    public string? Endpoint { get; init; }
    public TokenCredential? Credential { get; init; }
    public IEmbeddingGenerator? EmbeddingGenerator { get; init; }
}

public sealed class VectorStoreBuilder
{
    public string Provider => "azure-ai-search";

    private readonly AzureAISearchVectorStoreOptionsConfig _config;

    public VectorStoreBuilder(IOptions<AzureAISearchVectorStoreOptionsConfig> config)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
    }

    public VectorStore Build()
    {
        Validate();

        var client = new SearchIndexClient(new Uri(_config.Endpoint!), _config.Credential!);

        var options = CreateOptions(_config.EmbeddingGenerator!);

        return new AzureAISearchVectorStore(client, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_config.Endpoint))
        {
            throw new InvalidOperationException("Azure AI Search endpoint is not configured.");
        }

        if (_config.Credential is null)
        {
            throw new InvalidOperationException("Azure AI Search credential is not configured.");
        }

        if (_config.EmbeddingGenerator is null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private static AzureAISearchVectorStoreOptions CreateOptions(
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
    private string? _endpoint;
    private TokenCredential? _credential;
    private IEmbeddingGenerator? _embeddingGenerator;
    private Action<AzureAISearchVectorStoreOptions>? _configureOptions;

    public AzureAISearchVectorStoreBuilder WithEndpoint(string endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        return this;
    }

    public AzureAISearchVectorStoreBuilder WithCredential(TokenCredential credential)
    {
        _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        return this;
    }

    public AzureAISearchVectorStoreBuilder WithEmbeddingGenerator(IEmbeddingGenerator embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        return this;
    }

    public AzureAISearchVectorStoreBuilder WithOptions(Action<AzureAISearchVectorStoreOptions> configure)
    {
        _configureOptions = configure;
        return this;
    }

    public VectorStore Build()
    {
        Validate();

        var client = new SearchIndexClient(new Uri(_endpoint!), _credential!);

        var options = CreateOptions(_embeddingGenerator!, _configureOptions);

        return new AzureAISearchVectorStore(client, options);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
        {
            throw new InvalidOperationException("Azure AI Search endpoint is not configured.");
        }

        if (_credential is null)
        {
            throw new InvalidOperationException("Azure AI Search credential is not configured.");
        }

        if (_embeddingGenerator is null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private static AzureAISearchVectorStoreOptions CreateOptions(
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

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureAISearchVectorStore(
        this IServiceCollection services,
        Action<AzureAISearchVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<VectorStoreBuilder>();

        return services;
    }

    public static IServiceCollection AddAzureAISearchVectorStore(
        this IServiceCollection services,
        VectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new AzureAISearchFactory(store);

        services.AddSingleton<VectorStoreBuilder>(factory);

        return services;
    }

    private sealed class AzureAISearchFactory
    {
        private readonly VectorStore _store;

        public AzureAISearchFactory(VectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "azure-ai-search";

        public VectorStore Build()
        {
            return _store;
        }
    }
}
