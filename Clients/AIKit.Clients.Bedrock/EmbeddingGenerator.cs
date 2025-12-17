using AIKit.Core.Clients;
using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Bedrock;

public sealed class EmbeddingGeneratorFactory : IEmbeddingGeneratorProvider
{
    private readonly AIClientSettings _defaultSettings;

    public EmbeddingGeneratorFactory(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "aws-bedrock";

    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    public IEmbeddingGenerator<string, Embedding<float>> Create(AIClientSettings settings)
    {
        Validate(settings);

        var regionEndpoint = RegionEndpoint.GetBySystemName(settings.AwsRegion!);

        IAmazonBedrockRuntime runtime = new AmazonBedrockRuntimeClient(
            settings.AwsAccessKey!,
            settings.AwsSecretKey!,
            regionEndpoint);

        return runtime.AsIEmbeddingGenerator(settings.ModelId!);
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}