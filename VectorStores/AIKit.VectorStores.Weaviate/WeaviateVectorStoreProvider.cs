using AIKit.Core.VectorStores;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Weaviate;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AIKit.VectorStores.Weaviate;

public sealed class WeaviateVectorStoreOptionsConfig
{
    public required Uri Endpoint { get; init; }
    public string? ApiKey { get; init; }
    public int? TimeoutSeconds { get; init; }
}

public sealed class WeaviateVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "weaviate";

    private readonly WeaviateVectorStoreOptionsConfig _config;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public WeaviateVectorStoreFactory(
        IOptions<WeaviateVectorStoreOptionsConfig> config,
        IEmbeddingGenerator embeddingGenerator)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
    }

    /// <summary>
    /// Creates a vector store with full configuration from settings.
    /// </summary>
    public VectorStore Create()
    {
        if (_config.Endpoint is null)
        {
            throw new InvalidOperationException("Weaviate endpoint is not configured.");
        }

        var client = CreateHttpClient(_config);

        var options = WeaviateVectorStoreOptionsFactory.Create(_embeddingGenerator);

        return new WeaviateVectorStore(client, options);
    }

    private static HttpClient CreateHttpClient(WeaviateVectorStoreOptionsConfig config)
    {
        var client = new HttpClient { BaseAddress = config.Endpoint };

        if (!string.IsNullOrEmpty(config.ApiKey))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        }

        if (config.TimeoutSeconds.HasValue)
        {
            client.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds.Value);
        }

        return client;
    }
}

internal static class WeaviateVectorStoreOptionsFactory
{
    public static WeaviateVectorStoreOptions Create(IEmbeddingGenerator embeddingGenerator)
    {
        return new WeaviateVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}

public sealed class WeaviateVectorStoreBuilder
{
    private Uri? _endpoint;
    private string? _apiKey;
    private int? _timeoutSeconds;
    private WeaviateVectorStoreOptions? _options;

    public WeaviateVectorStoreBuilder WithEndpoint(Uri endpoint)
    {
        _endpoint = endpoint;
        return this;
    }

    public WeaviateVectorStoreBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey;
        return this;
    }

    public WeaviateVectorStoreBuilder WithTimeout(int timeoutSeconds)
    {
        _timeoutSeconds = timeoutSeconds;
        return this;
    }

    public WeaviateVectorStoreBuilder WithOptions(WeaviateVectorStoreOptions options)
    {
        _options = options;
        return this;
    }

    public VectorStore Build()
    {
        if (_endpoint is null)
            throw new InvalidOperationException("Endpoint must be set.");

        if (_options is null)
            throw new InvalidOperationException("WeaviateVectorStoreOptions must be set.");

        var client = new HttpClient { BaseAddress = _endpoint };

        if (!string.IsNullOrEmpty(_apiKey))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        if (_timeoutSeconds.HasValue)
        {
            client.Timeout = TimeSpan.FromSeconds(_timeoutSeconds.Value);
        }

        return new WeaviateVectorStore(client, _options);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWeaviateVectorStore(
        this IServiceCollection services,
        Action<WeaviateVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<IVectorStoreFactory, WeaviateVectorStoreFactory>();

        return services;
    }

    public static IServiceCollection AddWeaviateVectorStore(
        this IServiceCollection services,
        WeaviateVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new WeaviateFactory(store);

        services.AddSingleton<IVectorStoreFactory>(factory);

        return services;
    }

    private sealed class WeaviateFactory : IVectorStoreFactory
    {
        private readonly WeaviateVectorStore _store;

        public WeaviateFactory(WeaviateVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "weaviate";

        public VectorStore Create()
        {
            return _store;
        }
    }
}