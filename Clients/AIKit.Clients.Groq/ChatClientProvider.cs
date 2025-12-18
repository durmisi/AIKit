using AIKit.Core.Clients;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.Groq;

public sealed class ChatClientProvider : IChatClientProvider
{
    private readonly AIClientSettings _defaultSettings;

    public ChatClientProvider(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => _defaultSettings.ProviderName ?? "groq";

    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://api.groq.com/openai/v1/")
        };

        var credential = new ApiKeyCredential(settings.ApiKey!);
        var client = new OpenAIClient(credential, options);

        var targetModel = model ?? settings.ModelId!;

        return client.GetChatClient(targetModel).AsIChatClient();
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}