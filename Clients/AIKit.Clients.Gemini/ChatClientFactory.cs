using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.Gemini;

public sealed class ChatClientFactory : BaseChatClientFactory
{
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
        : base(settings, logger)
    {
    }

    protected override string GetDefaultProviderName() => "gemini";

    protected override void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }

    protected override IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
        var options = new GeminiClientOptions
        {
            ApiKey = settings.ApiKey!,
            ModelId = modelName ?? settings.ModelId!
        };

        var targetModel = modelName ?? settings.ModelId!;
        _logger?.LogInformation("Creating Gemini chat client for model {Model}", targetModel);

        return new GeminiChatClient(options);
    }
}