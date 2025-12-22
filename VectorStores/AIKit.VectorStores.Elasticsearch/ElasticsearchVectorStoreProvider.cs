using AIKit.Core.VectorStores;
using Elastic.Clients.Elasticsearch;
using Elastic.SemanticKernel.Connectors.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace AIKit.VectorStores.Elasticsearch;

public sealed class ElasticsearchVectorStoreProvider : IVectorStoreProvider
{
    public string Provider => "elasticsearch";

    public VectorStore Create()
        => throw new InvalidOperationException(
            "ElasticsearchVectorStoreProvider requires VectorStoreSettings");

    public VectorStore Create(VectorStoreSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentException.ThrowIfNullOrWhiteSpace(settings.Endpoint);

        var client = CreateElasticsearchClient(settings);

#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var options = new ElasticsearchVectorStoreOptions()
        {
            EmbeddingGenerator = settings.EmbeddingGenerator ?? throw new Exception(""),
        };

        return new ElasticsearchVectorStore(client,ownsClient: true, options);

#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }

    private static ElasticsearchClient CreateElasticsearchClient(VectorStoreSettings settings)
    {
        var settingsBuilder = new ElasticsearchClientSettings(new Uri(settings.Endpoint));

        // TODO: Add authentication options when available
        // Authentication classes may not be in this version

        return new ElasticsearchClient(settingsBuilder);
    }
}