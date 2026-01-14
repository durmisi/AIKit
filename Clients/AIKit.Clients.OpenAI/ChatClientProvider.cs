using AIKit.Core.Clients;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.OpenAI;

public sealed class ChatClientProvider : IChatClientProvider
{
    private readonly AIClientSettings _defaultSettings;
    private readonly ILogger<ChatClientProvider>? _logger;

    public ChatClientProvider(AIClientSettings settings, ILogger<ChatClientProvider>? logger = null)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;

        Validate(_defaultSettings);
    }

    public string Provider => _defaultSettings.ProviderName ?? "open-ai";

    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

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

        var credential = new ApiKeyCredential(settings.ApiKey!);
        var client = new OpenAIClient(credential, options);

        var targetModel = model ?? settings.ModelId!;
        _logger?.LogInformation("Creating OpenAI chat client for model {Model}", targetModel);

        return client
            .GetChatClient(targetModel)
            .AsIChatClient();
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}
