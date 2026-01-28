using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Claude;

/// <summary>
/// Factory for creating Claude chat clients.
/// </summary>
public sealed class ChatClientFactory : IChatClientFactory
{
    private readonly AIClientSettings _defaultSettings;
    private readonly ILogger<ChatClientFactory>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatClientFactory"/> class.
    /// </summary>
    /// <param name="settings">The default settings for creating clients.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown if settings is null.</exception>
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;

        Validate(_defaultSettings);
    }

    /// <summary>
    /// Gets the provider name for this factory.
    /// </summary>
    public string Provider => _defaultSettings.ProviderName ?? "claude";

    /// <summary>
    /// Creates a chat client using the default settings.
    /// </summary>
    /// <param name="model">Optional model name to override the default.</param>
    /// <returns>An <see cref="IChatClient"/> instance configured for Claude.</returns>
    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    /// <summary>
    /// Creates a chat client with custom settings.
    /// </summary>
    /// <param name="settings">The settings to use for the client.</param>
    /// <param name="model">Optional model name to override the settings.</param>
    /// <returns>An <see cref="IChatClient"/> instance configured for Claude.</returns>
    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

        AnthropicClient client = new(new Anthropic.Core.ClientOptions()
        {
            ApiKey = settings.ApiKey,
        });

        IChatClient chatClient = client.AsIChatClient(model)
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();

        return chatClient;
    }

    /// <summary>
    /// Validates the provided settings for Claude client creation.
    /// </summary>
    /// <param name="settings">The settings to validate.</param>
    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}