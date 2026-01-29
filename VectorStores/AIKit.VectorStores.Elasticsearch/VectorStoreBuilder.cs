using Elastic.Clients.Elasticsearch;
using Elastic.SemanticKernel.Connectors.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace AIKit.VectorStores.Elasticsearch;

public sealed class VectorStoreBuilder
{
    public string Provider => "elasticsearch";

    private Uri? _endpoint;
    private string? _username;
    private string? _password;
    private string? _apiKey;
    private IEmbeddingGenerator? _embeddingGenerator;

    public VectorStoreBuilder()
    {
    }

    public VectorStoreBuilder WithEndpoint(Uri endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        return this;
    }

    public VectorStoreBuilder WithBasicAuthentication(string username, string password)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _password = password ?? throw new ArgumentNullException(nameof(password));
        return this;
    }

    public VectorStoreBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        return this;
    }

    public VectorStoreBuilder WithEmbeddingGenerator(IEmbeddingGenerator embeddingGenerator)
    {
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        return this;
    }

    public VectorStore Build()
    {
        Validate();

        var settingsBuilder = new ElasticsearchClientSettings(_endpoint!);

        // Configure authentication if provided
        if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
        {
            settingsBuilder = settingsBuilder.Authentication(new BasicAuthentication(_username, _password));
        }
        if (!string.IsNullOrEmpty(_apiKey))
        {
            settingsBuilder = settingsBuilder.Authentication(new ApiKey(_apiKey));
        }

        var client = new ElasticsearchClient(settingsBuilder);

        var options = CreateStoreOptions(_embeddingGenerator!);

        return new ElasticsearchVectorStore(client, ownsClient: true, options);
    }

    private ElasticsearchVectorStoreOptions? CreateStoreOptions(IEmbeddingGenerator embeddingGenerator)
    {
        return new ElasticsearchVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };
    }

    private void Validate()
    {
        if (_endpoint == null)
        {
            throw new InvalidOperationException("Elasticsearch endpoint is not configured.");
        }

        if (_embeddingGenerator == null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }
}
