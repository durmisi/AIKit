using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.AzureOpenAI;

public sealed class EmbeddingGeneratorFactory : BaseEmbeddingGeneratorFactory
{
    public EmbeddingGeneratorFactory(AIClientSettings settings)
        : base(settings)
    {
    }

    protected override string GetDefaultProviderName() => "azure-open-ai";

    protected override void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);

        if (!settings.UseDefaultAzureCredential)
        {
            AIClientSettingsValidator.RequireApiKey(settings);
        }
    }

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