using AIKit.Core.Clients;
using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.AzureOpenAI;

public sealed class EmbeddingGenerator : IEmbeddingGeneratorProvider
{
    private readonly AIClientSettings _defaultSettings;

    public EmbeddingGenerator(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "azure-open-ai";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings)
    {
        Validate(settings);

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

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);

        if (!settings.UseDefaultAzureCredential)
        {
            AIClientSettingsValidator.RequireApiKey(settings);
        }
    }
}

