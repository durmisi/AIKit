using AIKit.Core.VectorStores;
using AIKit.Core.Vector;
using Elastic.Clients.Elasticsearch;
using Elastic.SemanticKernel.Connectors.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace AIKit.VectorStores.Elasticsearch;

public sealed class ElasticsearchVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "elasticsearch";

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var client = CreateElasticsearchClient(settings);

#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var options = new ElasticsearchVectorStoreOptions()
        {
            EmbeddingGenerator = VectorStoreProviderHelpers.ResolveEmbeddingGenerator(settings),
        };

        return new ElasticsearchVectorStore(client, ownsClient: true, options);

#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }

    /// <summary>
    /// Creates a vector store with minimal configuration using endpoint and embedding generator.
    /// </summary>
    public VectorStore Create(string endpoint, IEmbeddingGenerator embeddingGenerator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(embeddingGenerator);

        var settingsBuilder = new ElasticsearchClientSettings(new Uri(endpoint));
        var client = new ElasticsearchClient(settingsBuilder);

#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var options = new ElasticsearchVectorStoreOptions()
        {
            EmbeddingGenerator = embeddingGenerator,
        };

        return new ElasticsearchVectorStore(client, ownsClient: true, options);

#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }

    /// <summary>
    /// Creates a vector store with a pre-configured ElasticsearchClient and options.
    /// For advanced scenarios where full control over the Elasticsearch connection is needed.
    /// </summary>
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public VectorStore Create(ElasticsearchClient elasticsearchClient, ElasticsearchVectorStoreOptions options)
    {
        ArgumentNullException.ThrowIfNull(elasticsearchClient);
        ArgumentNullException.ThrowIfNull(options);

        return new ElasticsearchVectorStore(elasticsearchClient, ownsClient: true, options);
    }
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

    private static ElasticsearchClient CreateElasticsearchClient(VectorStoreSettings settings)
    {
        var settingsBuilder = new ElasticsearchClientSettings(new Uri(settings.Endpoint));

        // TODO: Add authentication options when available
        // Authentication classes may not be in this version

        return new ElasticsearchClient(settingsBuilder);
    }
}