using Elastic.Clients.Elasticsearch;
using Elastic.SemanticKernel.Connectors.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

namespace AIKit.VectorStores.Elasticsearch;

public sealed class ElasticsearchVectorStoreOptionsConfig
{
    public Uri Endpoint { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? ApiKey { get; init; }
}

public sealed class ElasticsearchVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "elasticsearch";

    private readonly ElasticsearchVectorStoreOptionsConfig _config;
    private readonly IEmbeddingGenerator _embeddingGenerator;

    public ElasticsearchVectorStoreFactory(
        IOptions<ElasticsearchVectorStoreOptionsConfig> config,
        IEmbeddingGenerator embeddingGenerator)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
    }

    public VectorStore Create()
    {
        if (_config.Endpoint is null)
        {
            throw new InvalidOperationException("Elasticsearch endpoint is not configured.");
        }

        var settingsBuilder = new ElasticsearchClientSettings(_config.Endpoint);

        // Configure authentication if provided
        if (!string.IsNullOrEmpty(_config.Username) && !string.IsNullOrEmpty(_config.Password))
        {
            settingsBuilder = settingsBuilder.Authentication(new BasicAuthentication(_config.Username, _config.Password));
        }
        if (!string.IsNullOrEmpty(_config.ApiKey))
        {
            settingsBuilder = settingsBuilder.Authentication(new ApiKey(_config.ApiKey));
        }

        var client = new ElasticsearchClient(settingsBuilder);

#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var options = ElasticsearchVectorStoreOptionsFactory.Create(_embeddingGenerator);

        return new ElasticsearchVectorStore(client, ownsClient: true, options);
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }
}

internal static class ElasticsearchVectorStoreOptionsFactory
{
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public static ElasticsearchVectorStoreOptions Create(IEmbeddingGenerator embeddingGenerator)
    {
        return new ElasticsearchVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };
    }

#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed
}

public sealed class ElasticsearchVectorStoreBuilder
{
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private ElasticsearchVectorStoreOptions? _options;
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed

    private Uri? _endpoint;
    private ElasticsearchClientSettings? _clientSettings;
    private string? _username;
    private string? _password;

    public ElasticsearchVectorStoreBuilder WithEndpoint(Uri endpoint)
    {
        _endpoint = endpoint;
        return this;
    }

    public ElasticsearchVectorStoreBuilder WithClientSettings(ElasticsearchClientSettings clientSettings)
    {
        _clientSettings = clientSettings;
        return this;
    }

    public ElasticsearchVectorStoreBuilder WithBasicAuthentication(string username, string password)
    {
        _username = username;
        _password = password;
        return this;
    }

#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    public ElasticsearchVectorStoreBuilder WithOptions(ElasticsearchVectorStoreOptions options)
    {
        _options = options;
        return this;
    }

#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed

    public VectorStore Build()
    {
        if (_endpoint is null)
            throw new InvalidOperationException("Endpoint must be set.");

#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        if (_options is null)
            throw new InvalidOperationException("ElasticsearchVectorStoreOptions must be set.");
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed

        var settings = _clientSettings ?? new ElasticsearchClientSettings(_endpoint);

        var client = new ElasticsearchClient(settings);

#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        return new ElasticsearchVectorStore(client, ownsClient: true, _options);
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddElasticsearchVectorStore(
        this IServiceCollection services,
        Action<ElasticsearchVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<IVectorStoreFactory, ElasticsearchVectorStoreFactory>();

        return services;
    }

    public static IServiceCollection AddElasticsearchVectorStore(
        this IServiceCollection services,
        ElasticsearchVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new ElasticsearchFactory(store);

        services.AddSingleton<IVectorStoreFactory>(factory);

        return services;
    }

    private sealed class ElasticsearchFactory : IVectorStoreFactory
    {
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed
        private readonly ElasticsearchVectorStore _store;
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed

        public ElasticsearchFactory(ElasticsearchVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "elasticsearch";

        public VectorStore Create()
        {
            return _store;
        }
    }
}