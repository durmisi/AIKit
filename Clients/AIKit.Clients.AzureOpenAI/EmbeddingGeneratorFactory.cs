using AIKit.Clients.Interfaces;
using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.AzureOpenAI;

/// <summary>
/// Factory for creating Azure OpenAI embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorFactory
{
    private readonly Dictionary<string, object> _defaultSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorFactory"/> class.
    /// </summary>
    /// <param name="settings">The settings as key-value pairs.</param>
    public EmbeddingGeneratorFactory(Dictionary<string, object> settings)
    {
        _defaultSettings = settings ?? throw new ArgumentNullException(nameof(settings));
        Validate(_defaultSettings);
    }

    /// <summary>
    /// Gets the provider name for this factory using the default settings.
    /// </summary>
    public string Provider => GetProviderName(_defaultSettings);

    /// <summary>
    /// Creates an embedding generator using the default settings.
    /// </summary>
    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    /// <summary>
    /// Creates an embedding generator with the specified settings.
    /// </summary>
    public IEmbeddingGenerator<string, Embedding<float>> Create(Dictionary<string, object> settings)
    {
        Validate(settings);

        var generator = CreateGenerator(settings);

        if (settings.TryGetValue("RetryPolicy", out var retryObj) && retryObj is RetryPolicySettings retryPolicy)
        {
            return new RetryEmbeddingGenerator(generator, retryPolicy);
        }

        return generator;
    }

    /// <summary>
    /// Gets the provider name from settings.
    /// </summary>
    /// <param name="settings">The settings dictionary.</param>
    /// <returns>The provider name.</returns>
    private string GetProviderName(Dictionary<string, object> settings)
        => settings.TryGetValue("ProviderName", out var provider) && provider is string s ? s : "azure-open-ai";

    /// <summary>
    /// Validates the settings for this provider.
    /// </summary>
    /// <param name="settings">The settings to validate.</param>
    private void Validate(Dictionary<string, object> settings)
    {
        if (!settings.TryGetValue("Endpoint", out var endpoint) || string.IsNullOrWhiteSpace(endpoint as string))
            throw new ArgumentException("Endpoint is required.", "Endpoint");

        if (!Uri.TryCreate(endpoint as string, UriKind.Absolute, out _))
            throw new ArgumentException("Endpoint must be a valid absolute URI.", "Endpoint");

        if (!settings.TryGetValue("ModelId", out var modelId) || string.IsNullOrWhiteSpace(modelId as string))
            throw new ArgumentException("ModelId is required.", "ModelId");

        var useDefault = settings.TryGetValue("UseDefaultAzureCredential", out var useDef) && useDef is bool b && b;
        if (!useDefault)
        {
            if (!settings.TryGetValue("ApiKey", out var apiKey) || string.IsNullOrWhiteSpace(apiKey as string))
                throw new ArgumentException("ApiKey is required when not using default Azure credential.", "ApiKey");
        }
    }

    /// <summary>
    /// Creates the actual embedding generator instance.
    /// </summary>
    /// <param name="settings">The settings as key-value pairs.</param>
    /// <returns>The created embedding generator.</returns>
    private IEmbeddingGenerator<string, Embedding<float>> CreateGenerator(Dictionary<string, object> settings)
    {
        EmbeddingsClient client;

        var useDefault = settings.TryGetValue("UseDefaultAzureCredential", out var useDef) && useDef is bool b && b;
        var endpoint = (string)settings["Endpoint"];

        if (useDefault)
        {
            client = new EmbeddingsClient(
                new Uri(endpoint),
                new DefaultAzureCredential());
        }
        else
        {
            var apiKey = (string)settings["ApiKey"];
            client = new EmbeddingsClient(
                new Uri(endpoint),
                new AzureKeyCredential(apiKey));
        }

        var modelId = (string)settings["ModelId"];
        return client.AsIEmbeddingGenerator(modelId);
    }
}