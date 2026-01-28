using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.OpenAI;

/// <summary>
/// Factory for creating OpenAI embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorFactory : BaseEmbeddingGeneratorFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorFactory"/> class.
    /// </summary>
    /// <param name="settings">The default settings for creating generators.</param>
    /// <exception cref="ArgumentNullException">Thrown if settings is null.</exception>
    public EmbeddingGeneratorFactory(AIClientSettings settings)
        : base(settings)
    {
    }

    /// <summary>
    /// Gets the default provider name.
    /// </summary>
    /// <returns>The default provider name.</returns>
    protected override string GetDefaultProviderName() => "open-ai";

    /// <summary>
    /// Validates the provided settings for OpenAI embedding generator creation.
    /// </summary>
    /// <param name="settings">The settings to validate.</param>
    protected override void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }

    /// <summary>
    /// Creates the actual embedding generator instance.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <returns>The created embedding generator.</returns>
    protected override IEmbeddingGenerator<string, Embedding<float>> CreateGenerator(AIClientSettings settings)
    {
        var options = new OpenAIClientOptions();
        if (!string.IsNullOrWhiteSpace(settings.Organization))
        {
            options.OrganizationId = settings.Organization;
        }

        var credential = new ApiKeyCredential(settings.ApiKey!);
        var client = new OpenAIClient(credential, options);
        return client.GetEmbeddingClient(settings.ModelId!).AsIEmbeddingGenerator();
    }
}