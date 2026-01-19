using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace AIKit.VectorStores.Qdrant;

public sealed class QdrantVectorStoreOptionsConfig
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 6334;
    public string? ApiKey { get; init; }
}

public sealed class QdrantVectorStoreFactory : IVectorStoreFactory
{
    public string Provider => "qdrant";

    private readonly QdrantVectorStoreOptionsConfig _config;

    public QdrantVectorStoreFactory(IOptions<QdrantVectorStoreOptionsConfig> config)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
    }

    public VectorStore Create()
    {
        if (string.IsNullOrWhiteSpace(_config.Host))
        {
            throw new InvalidOperationException("Qdrant host is not configured.");
        }

        var client = new QdrantClient(_config.Host, _config.Port, apiKey: _config.ApiKey);

        return new QdrantVectorStore(client, ownsClient: true);
    }
}

public sealed class QdrantVectorStoreBuilder
{
    private string? _host;
    private int _port = 6334;
    private string? _apiKey;
    private QdrantClient? _client;

    public QdrantVectorStoreBuilder WithHost(string host)
    {
        _host = host;
        return this;
    }

    public QdrantVectorStoreBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    public QdrantVectorStoreBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey;
        return this;
    }

    public QdrantVectorStoreBuilder WithClient(QdrantClient client)
    {
        _client = client;
        return this;
    }

    public VectorStore Build()
    {
        if (_client is not null)
        {
            return new QdrantVectorStore(_client, ownsClient: true);
        }

        if (string.IsNullOrWhiteSpace(_host))
            throw new InvalidOperationException("Host must be set if no client is provided.");

        var client = new QdrantClient(_host, _port, apiKey: _apiKey);
        return new QdrantVectorStore(client, ownsClient: true);
    }
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQdrantVectorStore(
        this IServiceCollection services,
        Action<QdrantVectorStoreOptionsConfig> configure)
    {
        // Configure options
        services.Configure(configure);

        // Register the factory
        services.AddSingleton<IVectorStoreFactory, QdrantVectorStoreFactory>();

        return services;
    }

    public static IServiceCollection AddQdrantVectorStore(
        this IServiceCollection services,
        QdrantVectorStore store)
    {
        if (store == null) throw new ArgumentNullException(nameof(store));

        var factory = new QdrantFactory(store);

        services.AddSingleton<IVectorStoreFactory>(factory);

        return services;
    }

    private sealed class QdrantFactory : IVectorStoreFactory
    {
        private readonly QdrantVectorStore _store;

        public QdrantFactory(QdrantVectorStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public string Provider => "qdrant";

        public VectorStore Create()
        {
            return _store;
        }
    }
}