using AIKit.Clients.Interfaces;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Ollama;

/// <summary>
/// Requires Endpoint (e.g., http://localhost:11434), ModelId. No ApiKey needed
/// </summary>
public sealed class ChatClientFactory : IChatClientFactory
{
    private readonly AIClientSettings _defaultSettings;
    private readonly ILogger<ChatClientFactory>? _logger;

    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger;

        Validate(_defaultSettings);
    }

    public string Provider => _defaultSettings.ProviderName ?? "ollama";

    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

        var endpoint = new Uri(settings.Endpoint!);
        var targetModel = model ?? settings.ModelId!;
        _logger?.LogInformation("Creating Ollama chat client for model {Model} at {Endpoint}", targetModel, endpoint);

        return new OllamaChatClient(endpoint, targetModel);
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}