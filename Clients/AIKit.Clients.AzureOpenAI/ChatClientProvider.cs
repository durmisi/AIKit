using AIKit.Core.Clients;
using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace AIKit.Clients.AzureOpenAI;

public sealed class ChatClientProvider : IChatClientProvider
{
    private readonly AIClientSettings _defaultSettings;

    public ChatClientProvider(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "azure-open-ai";

    public IChatClient Create(string? model = null)
        => Create(_defaultSettings, model);

    public IChatClient Create(AIClientSettings settings, string? model = null)
    {
        Validate(settings);

        ChatCompletionsClient? client = null;

        if (settings.UseDefaultAzureCredential)
        {
            client = new ChatCompletionsClient(
                    new Uri(settings.Endpoint!),
                    new DefaultAzureCredential());
        }
        else
        {
            client = new ChatCompletionsClient(
                new Uri(settings.Endpoint!),
                new AzureKeyCredential(settings.ApiKey!));

        }

        var targetModel = model ?? settings.ModelId;    

        return client.AsIChatClient(targetModel);
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);

        if (!settings.UseDefaultAzureCredential)
        {
            AIClientSettingsValidator.RequireApiKey(settings);
        }
    }
}
