using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.OpenAI;

public sealed class EmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    private readonly AIClientSettings _defaultSettings;

    public EmbeddingGeneratorFactory(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "open-ai";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

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
        return client.GetEmbeddingClient(settings.ModelId!).AsIEmbeddingGenerator();
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}
