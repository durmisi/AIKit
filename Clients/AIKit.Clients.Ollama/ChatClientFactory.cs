using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Ollama;

/// <summary>
/// Requires Endpoint (e.g., http://localhost:11434), ModelId. No ApiKey needed
/// </summary>
public sealed class ChatClientFactory : BaseChatClientFactory
{
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
        : base(settings, logger)
    {
    }

    protected override string GetDefaultProviderName() => "ollama";

    protected override void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }

    protected override IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
        var endpoint = new Uri(settings.Endpoint!);
        var targetModel = modelName ?? settings.ModelId!;
        _logger?.LogInformation("Creating Ollama chat client for model {Model} at {Endpoint}", targetModel, endpoint);

        return new OllamaChatClient(endpoint, targetModel);
    }
}