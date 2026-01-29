using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Weaviate;

namespace AIKit.VectorStores.Weaviate;

public record HttpClientOptions
{
    public Uri? Endpoint { get; init; }
    public string? ApiKey { get; init; }
    public int? TimeoutSeconds { get; init; }
}

public sealed class VectorStoreBuilder
{
    public string Provider => "weaviate";

    private Uri? _endpoint;
    private string? _apiKey;
    private int? _timeoutSeconds;
    private IEmbeddingGenerator? _embeddingGenerator;
    private HttpClient? _httpClient;

    public VectorStoreBuilder()
    {
    }

    public VectorStoreBuilder WithHttpClient(HttpClient httpClient, HttpClientOptions? options = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        if (options != null)
        {
            _endpoint = options.Endpoint;
            _apiKey = options.ApiKey;
            _timeoutSeconds = options.TimeoutSeconds;

            if (_endpoint != null)
            {
                _httpClient.BaseAddress = _endpoint;
            }

            if (!string.IsNullOrEmpty(_apiKey) && !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }

            if (_timeoutSeconds.HasValue)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds.Value);
            }
        }

        return this;
    }

    public VectorStore Build()
    {
        Validate();

        var options = CreateStoreOptions(_embeddingGenerator!);

        return new WeaviateVectorStore(_httpClient!, options);
    }

    private void Validate()
    {
        if (_httpClient == null)
        {
            throw new InvalidOperationException("HttpClient is not configured.");
        }

        if (_embeddingGenerator == null)
        {
            throw new InvalidOperationException("Embedding generator is not configured.");
        }
    }

    private WeaviateVectorStoreOptions? CreateStoreOptions(IEmbeddingGenerator embeddingGenerator)
    {
        return new WeaviateVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator,
        };
    }
}

