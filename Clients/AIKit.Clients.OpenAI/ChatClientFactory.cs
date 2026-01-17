using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace AIKit.Clients.OpenAI;

/// <summary>
/// Factory for creating OpenAI chat clients.
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
    public string Provider => _defaultSettings.ProviderName ?? "open-ai";

    /// <summary>
    /// Creates a chat client using the default settings.
    /// </summary>
    /// <param name="model">Optional model name to override the default.</param>
    /// <returns>An <see cref="IChatClient"/> instance configured for OpenAI.</returns>
    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    /// <summary>
    /// Creates a chat client with custom settings.
    /// </summary>
    /// <param name="settings">The settings to use for the client.</param>
    /// <param name="model">Optional model name to override the settings.</param>
    /// <returns>An <see cref="IChatClient"/> instance configured for OpenAI.</returns>
    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

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

        var credential = new ApiKeyCredential(settings.ApiKey!);
        var client = new OpenAIClient(credential, options);

        var targetModel = model ?? settings.ModelId!;
        _logger?.LogInformation("Creating OpenAI chat client for model {Model}", targetModel);

        var chatClient = client
            .GetChatClient(targetModel)
            .AsIChatClient();

        if (settings.RetryPolicy != null)
        {
            _logger?.LogInformation("Applying retry policy with {MaxRetries} max retries", settings.RetryPolicy.MaxRetries);
            return new RetryChatClient(chatClient, settings.RetryPolicy);
        }

        return chatClient;
    }

    /// <summary>
    /// Validates the provided settings for OpenAI client creation.
    /// </summary>
    /// <param name="settings">The settings to validate.</param>
    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}