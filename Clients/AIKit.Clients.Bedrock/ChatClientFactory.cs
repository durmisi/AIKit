using AIKit.Clients.Interfaces;
using AIKit.Clients.Settings;
using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Bedrock;

/// <summary>
/// Factory for creating AWS Bedrock chat clients.
/// </summary>
public sealed class ChatClientFactory : IChatClientFactory
{
    private readonly AIClientSettings _defaultSettings;
    private readonly ILogger<ChatClientFactory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatClientFactory"/> class.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <param name="logger">Optional logger instance.</param>
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;

        Validate(_defaultSettings);
    }

    /// <summary>
    /// Gets the provider name.
    /// </summary>
    public string Provider => _defaultSettings.ProviderName ?? "aws-bedrock";

    /// <summary>
    /// Creates a chat client using the default settings.
    /// </summary>
    /// <param name="model">Optional model name to use for the client.</param>
    /// <returns>The created chat client.</returns>
    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    /// <summary>
    /// Creates a chat client with the specified settings.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <param name="model">Optional model name to use for the client.</param>
    /// <returns>The created chat client.</returns>
    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

        var regionEndpoint = RegionEndpoint.GetBySystemName(settings.AwsRegion!);

        IAmazonBedrockRuntime runtime = new AmazonBedrockRuntimeClient(
            settings.AwsAccessKey!,
            settings.AwsSecretKey!,
            regionEndpoint);

        var targetModel = model ?? settings.ModelId;
        _logger?.LogInformation("Creating AWS Bedrock chat client for model {Model} in region {Region}", targetModel, settings.AwsRegion);

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