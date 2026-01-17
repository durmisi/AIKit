using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.OpenAI;

/// <summary>
/// Factory for creating OpenAI embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    private readonly AIClientSettings _defaultSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorFactory"/> class.
    /// </summary>
    /// <param name="settings">The default settings for creating generators.</param>
    /// <exception cref="ArgumentNullException">Thrown if settings is null.</exception>
    public EmbeddingGeneratorFactory(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    /// <summary>
    /// Gets the provider name for this factory.
    /// </summary>
    public string Provider => "open-ai";

    /// <summary>
    /// Creates an embedding generator using the default settings.
    /// </summary>
    /// <returns>An <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> instance configured for OpenAI.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    /// <summary>
    /// Creates an embedding generator with custom settings.
    /// </summary>
    /// <param name="settings">The settings to use for the generator.</param>
    /// <returns>An <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> instance configured for OpenAI.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings)
    {
        Validate(settings);

        var options = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(settings.Organization))
        {
            options.OrganizationId = settings.Organization;
        }

        var credential = new ApiKeyCredential(settings.ApiKey!);
        var client = new OpenAIClient(credential, options);
        var generator = client.GetEmbeddingClient(settings.ModelId!).AsIEmbeddingGenerator();

        if (settings.RetryPolicy != null)
        {
            return new RetryEmbeddingGenerator(generator, settings.RetryPolicy);
        }

        return generator;
    }

    /// <summary>
    /// Validates the provided settings for OpenAI embedding generator creation.
    /// </summary>
    /// <param name="settings">The settings to validate.</param>
    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}