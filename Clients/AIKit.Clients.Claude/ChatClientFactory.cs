using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Claude;

/// <summary>
/// Factory for creating Claude chat clients.
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
    protected override string GetDefaultProviderName() => "claude";

    /// <summary>
    /// Validates the provided settings for Claude client creation.
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
        AnthropicClient client = new(new Anthropic.Core.ClientOptions()
        {
            ApiKey = settings.ApiKey,
            Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds),
        });

        IChatClient chatClient = client.AsIChatClient(modelName)
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();

        return chatClient;
    }
}