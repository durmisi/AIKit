using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.AzureOpenAI;

/// <summary>
/// Factory for creating Azure OpenAI embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorFactory : BaseEmbeddingGeneratorFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorFactory"/> class.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    public EmbeddingGeneratorFactory(AIClientSettings settings)
        : base(settings)
    {
    }

    /// <summary>
    /// Gets the default provider name.
    /// </summary>
    /// <returns>The default provider name.</returns>
    protected override string GetDefaultProviderName() => "azure-open-ai";

    /// <summary>
    /// Validates the settings for this provider.
    /// </summary>
    /// <param name="settings">The AI client settings to validate.</param>
    protected override void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);

        if (!settings.UseDefaultAzureCredential)
        {
            AIClientSettingsValidator.RequireApiKey(settings);
        }
    }

    /// <summary>
    /// Creates the actual embedding generator instance.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <returns>The created embedding generator.</returns>
    protected override IEmbeddingGenerator<string, Embedding<float>> CreateGenerator(AIClientSettings settings)
    {
        EmbeddingsClient client;

        if (settings.UseDefaultAzureCredential)
        {
            client = new EmbeddingsClient(
                new Uri(settings.Endpoint!),
                new DefaultAzureCredential());
        }
        else
        {
            client = new EmbeddingsClient(
                new Uri(settings.Endpoint!),
                new AzureKeyCredential(settings.ApiKey!));
        }

        return client.AsIEmbeddingGenerator(settings.ModelId!);
    }
}