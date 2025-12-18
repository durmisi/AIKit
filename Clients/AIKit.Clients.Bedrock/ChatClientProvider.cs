using AIKit.Core.Clients;
using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Bedrock;

public sealed class ChatClientProvider : IChatClientProvider
{
    private readonly AIClientSettings _defaultSettings;

    public ChatClientProvider(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "aws-bedrock";

    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

        var regionEndpoint = RegionEndpoint.GetBySystemName(settings.AwsRegion!);

        IAmazonBedrockRuntime runtime = new AmazonBedrockRuntimeClient(
            settings.AwsAccessKey!,
            settings.AwsSecretKey!,
            regionEndpoint);

        var targetModel = model ?? settings.ModelId;    

        return runtime.AsIChatClient(targetModel);
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
