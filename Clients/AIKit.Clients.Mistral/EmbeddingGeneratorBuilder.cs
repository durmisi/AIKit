using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.Mistral;

/// <summary>
/// Builder for creating Mistral embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorBuilder
{
    private string? _apiKey;
    private string? _model;

    /// <summary>
    /// Sets the API key.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <returns>The builder instance.</returns>
    public EmbeddingGeneratorBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
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
    /// Builds the IEmbeddingGenerator instance.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Build()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("ApiKey is required. Call WithApiKey().");

        if (string.IsNullOrWhiteSpace(_model))
            throw new InvalidOperationException("Model is required. Call WithModel().");

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://api.mistral.ai/v1/")
        };

        var credential = new ApiKeyCredential(_apiKey);
        var client = new OpenAIClient(credential, options);

        return client.GetEmbeddingClient(_model).AsIEmbeddingGenerator();
    }
}