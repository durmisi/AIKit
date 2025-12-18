using AIKit.Core.Clients;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Ollama;

/// <summary>
/// Requires Endpoint (e.g., http://localhost:11434), ModelId. No ApiKey needed
/// </summary>
public sealed class ChatClientProvider : IChatClientProvider
{
    private readonly AIClientSettings _defaultSettings;

    public ChatClientProvider(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => _defaultSettings.ProviderName ?? "ollama";

    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

        var endpoint = new Uri(settings.Endpoint!);
        return new OllamaChatClient(endpoint, model ?? settings.ModelId!);
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}
