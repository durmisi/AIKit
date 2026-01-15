using AIKit.Core.VectorStores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace AIKit.VectorStores.Qdrant;

public sealed class QdrantVectorStoreOptionsConfig
{
    public required Uri Endpoint { get; init; }
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
        if (_config.Endpoint is null)
        {
            throw new InvalidOperationException("Qdrant endpoint is not configured.");
        }

        var client = new QdrantClient(_config.Endpoint.ToString());

        return new QdrantVectorStore(client, ownsClient: true);
    }
}

public sealed class QdrantVectorStoreBuilder
{
    private Uri? _endpoint;
    private QdrantClient? _client;

    public QdrantVectorStoreBuilder WithEndpoint(Uri endpoint)
    {
        _endpoint = endpoint;
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

        if (_endpoint is null)
            throw new InvalidOperationException("Endpoint must be set if no client is provided.");

        var client = new QdrantClient(_endpoint.ToString());
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