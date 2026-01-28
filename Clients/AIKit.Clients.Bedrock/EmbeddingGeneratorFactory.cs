using AIKit.Clients.Interfaces;
using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Bedrock;

/// <summary>
/// Factory for creating AWS Bedrock embedding generators.
/// </summary>
public sealed class EmbeddingGeneratorFactory : IEmbeddingGeneratorFactory
{
    private readonly Dictionary<string, object> _defaultSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddingGeneratorFactory"/> class.
    /// </summary>
    /// <param name="settings">The settings as key-value pairs.</param>
    public EmbeddingGeneratorFactory(Dictionary<string, object> settings)
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
    /// <param name="settings">The settings as key-value pairs.</param>
    /// <returns>The created embedding generator.</returns>
    public IEmbeddingGenerator<string, Embedding<float>> Create(Dictionary<string, object> settings)
    {
        Validate(settings);

        var region = (string)settings["AwsRegion"];
        var regionEndpoint = RegionEndpoint.GetBySystemName(region);

        var accessKey = (string)settings["AwsAccessKey"];
        var secretKey = (string)settings["AwsSecretKey"];
        IAmazonBedrockRuntime runtime = new AmazonBedrockRuntimeClient(
            accessKey,
            secretKey,
            regionEndpoint);

        var modelId = (string)settings["ModelId"];
        return runtime.AsIEmbeddingGenerator(modelId);
    }

    private static void Validate(Dictionary<string, object> settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (!settings.TryGetValue("AwsAccessKey", out var accessKey) || string.IsNullOrWhiteSpace(accessKey as string))
            throw new ArgumentException("AwsAccessKey is required.", "AwsAccessKey");

        if (!settings.TryGetValue("AwsSecretKey", out var secretKey) || string.IsNullOrWhiteSpace(secretKey as string))
            throw new ArgumentException("AwsSecretKey is required.", "AwsSecretKey");

        if (!settings.TryGetValue("AwsRegion", out var region) || string.IsNullOrWhiteSpace(region as string))
            throw new ArgumentException("AwsRegion is required.", "AwsRegion");

        if (!settings.TryGetValue("ModelId", out var modelId) || string.IsNullOrWhiteSpace(modelId as string))
            throw new ArgumentException("ModelId is required.", "ModelId");
    }
}