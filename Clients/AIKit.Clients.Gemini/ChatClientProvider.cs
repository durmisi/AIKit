using AIKit.Core.Clients;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Gemini;

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

    public string Provider => _defaultSettings.ProviderName ?? "gemini";

    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

        var options = new GeminiClientOptions
        {
            ApiKey = settings.ApiKey!,
            ModelId = model ?? settings.ModelId!
        };

        var targetModel = model ?? settings.ModelId!;
        _logger?.LogInformation("Creating Gemini chat client for model {Model}", targetModel);

        return new GeminiChatClient(options);
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}
