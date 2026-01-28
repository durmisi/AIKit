using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Bedrock;

/// <summary>
/// Factory for creating AWS Bedrock chat clients.
/// </summary>
public sealed class ChatClientFactory : BaseChatClientFactory
{
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
        : base(settings, logger)
    {
    }

    protected override string GetDefaultProviderName() => "aws-bedrock";

    protected override void Validate(AIClientSettings settings)
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

    protected override IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
        var regionEndpoint = RegionEndpoint.GetBySystemName(settings.AwsRegion!);

        IAmazonBedrockRuntime runtime = new AmazonBedrockRuntimeClient(
            settings.AwsAccessKey!,
            settings.AwsSecretKey!,
            regionEndpoint);

        var targetModel = modelName ?? settings.ModelId;
        _logger?.LogInformation("Creating AWS Bedrock chat client for model {Model} in region {Region}", targetModel, settings.AwsRegion);

        return runtime.AsIChatClient(targetModel);
    }
}