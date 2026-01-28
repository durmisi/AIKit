using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.AzureOpenAI;

/// <summary>
/// Factory for creating Azure OpenAI chat clients.
/// </summary>
public sealed class ChatClientFactory : BaseChatClientFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatClientFactory"/> class.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <param name="logger">Optional logger instance.</param>
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
        : base(settings, logger)
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
    /// Creates the actual chat client instance.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <param name="modelName">Optional model name.</param>
    /// <returns>The created chat client.</returns>
    protected override IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
        ChatCompletionsClient? client = null;

        if (settings.UseDefaultAzureCredential)
        {
            client = new ChatCompletionsClient(
                    new Uri(settings.Endpoint!),
                    new DefaultAzureCredential());
        }
        else
        {
            client = new ChatCompletionsClient(
                new Uri(settings.Endpoint!),
                new AzureKeyCredential(settings.ApiKey!));
        }

        var targetModel = modelName ?? settings.ModelId;
        _logger?.LogInformation("Creating Azure OpenAI chat client for model {Model}", targetModel);

        return client.AsIChatClient(targetModel);
    }
}