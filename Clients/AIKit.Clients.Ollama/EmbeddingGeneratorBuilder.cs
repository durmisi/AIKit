using AIKit.Clients.Interfaces;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Ollama;

/// <summary>
/// Builder for creating Ollama embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorBuilder 
{
    private string? _endpoint;
    private string? _model;
    private HttpClient? _httpClient;
    private string? _userAgent;
    private Dictionary<string, string>? _customHeaders;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorBuilder"/>.
    /// </summary>
    public EmbeddingGeneratorBuilder()
    {
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => "ollama";

    /// <summary>
    /// Sets the endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint URL.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithEndpoint(string? endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        return this;
    }

    /// <summary>
    /// Sets the model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithModel(string model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        return this;
    }

    /// <summary>
    /// Sets the HTTP client.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithHttpClient(HttpClient? httpClient)
    {
        _httpClient = httpClient;
        return this;
    }

    /// <summary>
    /// Sets the user agent.
    /// </summary>
    /// <param name="userAgent">The user agent string.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithUserAgent(string? userAgent)
    {
        _userAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Sets custom headers.
    /// </summary>
    /// <param name="headers">The custom headers.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithCustomHeaders(Dictionary<string, string>? headers)
    {
        _customHeaders = headers;
        return this;
    }

    /// <summary>
    /// Builds the IEmbeddingGenerator instance.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Build()
    {
        if (string.IsNullOrWhiteSpace(_endpoint))
            throw new InvalidOperationException("Endpoint is required. Call WithEndpoint().");

        if (string.IsNullOrWhiteSpace(_model))
            throw new InvalidOperationException("Model is required. Call WithModel().");

        if (_httpClient != null)
        {
            if (!string.IsNullOrEmpty(_userAgent))
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            }

            if (_customHeaders != null)
            {
                foreach (var kvp in _customHeaders)
                {
                    _httpClient.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }
            }
        }

        var uri = new Uri(_endpoint);
        return new OllamaEmbeddingGenerator(uri, _model, _httpClient);
    }
}