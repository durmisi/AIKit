using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace AIKit.Clients.OpenAI;

/// <summary>
/// Factory for creating OpenAI chat clients.
/// </summary>
public sealed class ChatClientFactory : BaseChatClientFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChatClientFactory"/> class.
    /// </summary>
    /// <param name="settings">The default settings for creating clients.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown if settings is null.</exception>
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
        : base(settings, logger)
    {
    }

    /// <summary>
    /// Gets the default provider name.
    /// </summary>
    /// <returns>The default provider name.</returns>
    protected override string GetDefaultProviderName() => "open-ai";

    /// <summary>
    /// Validates the provided settings for OpenAI client creation.
    /// </summary>
    /// <param name="settings">The settings to validate.</param>
    protected override void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }

    /// <summary>
    /// Creates the actual chat client instance.
    /// </summary>
    /// <param name="settings">The AI client settings.</param>
    /// <param name="modelName">Optional model name.</param>
    /// <returns>The created chat client.</returns>
    protected override IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
        var options = new OpenAIClientOptions();

        if (!string.IsNullOrEmpty(settings.Endpoint))
        {
            options.Endpoint = new Uri(settings.Endpoint!);
        }

        if (!string.IsNullOrWhiteSpace(settings.Organization))
        {
            options.OrganizationId = settings.Organization;
        }

        if (settings.HttpClient != null)
        {
            options.Transport = new HttpClientPipelineTransport(settings.HttpClient);
        }
        else
        {
            options.NetworkTimeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        }

        var credential = new ApiKeyCredential(settings.ApiKey!);
        var client = new OpenAIClient(credential, options);

        var targetModel = modelName ?? settings.ModelId!;
        _logger?.LogInformation("Creating OpenAI chat client for model {Model}", targetModel);

        return client
            .GetChatClient(targetModel)
            .AsIChatClient();
    }
}