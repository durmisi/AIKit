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

    public string Provider => "ollama";

    public IChatClient Create()
        => Create(_defaultSettings);

    public IChatClient Create(AIClientSettings settings)
    {
        Validate(settings);

        var endpoint = new Uri(settings.Endpoint!);
        return new OllamaChatClient(endpoint, settings.ModelId!);
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}
