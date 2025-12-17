using AIKit.Core.Clients;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.Gemini;

public sealed class ChatClientProvider : IChatClientProvider
{
    private readonly AIClientSettings _defaultSettings;

    public ChatClientProvider(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "gemini";

    public IChatClient Create()
        => Create(_defaultSettings);

    public IChatClient Create(AIClientSettings settings)
    {
        Validate(settings);

        var options = new GeminiClientOptions
        {
            ApiKey = settings.ApiKey!,
            ModelId = settings.ModelId!
        };

        return new GeminiChatClient(options);
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}
