using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace AIKit.VectorStores.Qdrant;

public sealed class VectorStoreBuilder
{
    public string Provider => "qdrant";

    private string _host = "localhost";
    private int _port = 6334;
    private string? _apiKey;
    private bool _ownsClient = true;

    public VectorStoreBuilder()
    {
    }

    public VectorStoreBuilder WithHost(string host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        return this;
    }

    public VectorStoreBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    public VectorStoreBuilder WithApiKey(string? apiKey)
    {
        _apiKey = apiKey;
        return this;
    }

    public VectorStoreBuilder WithOwnsClient(bool ownsClient)
    {
        _ownsClient = ownsClient;
        return this;
    }

    public VectorStore Build()
    {
        if (string.IsNullOrWhiteSpace(_host))
        {
            throw new InvalidOperationException("Qdrant host is not configured.");
        }

        var client = new QdrantClient(_host, _port, apiKey: _apiKey);

        return new QdrantVectorStore(client, ownsClient: _ownsClient);
    }
}
