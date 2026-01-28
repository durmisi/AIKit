using AIKit.Clients.Interfaces;
using AIKit.Clients.Settings;
using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Bedrock;

/// <summary>
/// Factory for creating AWS Bedrock embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    private readonly AIClientSettings _defaultSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorFactory"/> class.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    public EmbeddingGeneratorFactory(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => "aws-bedrock";

    /// <summary>
    /// Creates an embedding generator using the default settings.
    /// </summary>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Create() => Create(_defaultSettings);

    /// <summary>
    /// Creates an embedding generator with the specified settings.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <returns>The created embedding generator.</returns>
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
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.AwsAccessKey))
            throw new ArgumentException(
                "AwsAccessKey is required.",
                nameof(AIClientSettings.AwsAccessKey));

        if (string.IsNullOrWhiteSpace(settings.AwsSecretKey))
            throw new ArgumentException(
                "AwsSecretKey is required.",
                nameof(AIClientSettings.AwsSecretKey));

        if (string.IsNullOrWhiteSpace(settings.AwsRegion))
            throw new ArgumentException(
                "AwsRegion is required.",
                nameof(AIClientSettings.AwsRegion));

        AIClientSettingsValidator.RequireModel(settings);
    }
}