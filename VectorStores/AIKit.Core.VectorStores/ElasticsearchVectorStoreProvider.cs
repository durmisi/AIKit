using Elastic.Clients.Elasticsearch;
using Elastic.SemanticKernel.Connectors.Elasticsearch;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace AIKit.Core.Vector;

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

        var client = new ElasticsearchClient(new Uri(settings.Endpoint));

#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        return new ElasticsearchVectorStore(client, ownsClient: true);
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }
}