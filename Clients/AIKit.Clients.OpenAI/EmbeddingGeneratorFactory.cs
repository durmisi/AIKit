using AIKit.Clients.Interfaces;
using AIKit.Clients.Resilience;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.OpenAI;

/// <summary>
/// Factory for creating OpenAI embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorFactory
{
    private readonly Dictionary<string, object> _defaultSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorFactory"/> class.
    /// </summary>
    /// <param name="settings">The default settings for creating generators as key-value pairs.</param>
    /// <exception cref="ArgumentNullException">Thrown if settings is null.</exception>
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
        => settings.TryGetValue("ProviderName", out var provider) && provider is string s ? s : "open-ai";

    /// <summary>
    /// Validates the provided settings for OpenAI embedding generator creation.
    /// </summary>
    /// <param name="settings">The settings to validate.</param>
    private void Validate(Dictionary<string, object> settings)
    {
        if (!settings.TryGetValue("ApiKey", out var apiKey) || string.IsNullOrWhiteSpace(apiKey as string))
            throw new ArgumentException("ApiKey is required.", "ApiKey");

        if (!settings.TryGetValue("ModelId", out var modelId) || string.IsNullOrWhiteSpace(modelId as string))
            throw new ArgumentException("ModelId is required.", "ModelId");
    }

    /// <summary>
    /// Creates the actual embedding generator instance.
    /// </summary>
    /// <param name="settings">The settings as key-value pairs.</param>
    /// <returns>The created embedding generator.</returns>
    private IEmbeddingGenerator<string, Embedding<float>> CreateGenerator(Dictionary<string, object> settings)
    {
        var options = new OpenAIClientOptions();
        if (settings.TryGetValue("Organization", out var org) && org is string organization && !string.IsNullOrWhiteSpace(organization))
        {
            options.OrganizationId = organization;
        }

        var apiKey = (string)settings["ApiKey"];
        var credential = new ApiKeyCredential(apiKey);
        var client = new OpenAIClient(credential, options);
        var modelId = (string)settings["ModelId"];
        return client.GetEmbeddingClient(modelId).AsIEmbeddingGenerator();
    }
}