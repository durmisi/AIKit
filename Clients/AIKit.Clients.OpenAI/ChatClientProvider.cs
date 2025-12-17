using AIKit.Core.Clients;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

namespace AIKit.Clients.OpenAI;

public sealed class ChatClientProvider : IChatClientProvider
{
    private readonly AIClientSettings _defaultSettings;

    public ChatClientProvider(AIClientSettings settings)
    {
        _defaultSettings = settings
            ?? throw new ArgumentNullException(nameof(settings));

        Validate(_defaultSettings);
    }

    public string Provider => "open-ai";

    public IChatClient Create()
        => Create(_defaultSettings);

    public IChatClient Create(AIClientSettings settings)
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

        return client
            .GetChatClient(settings.ModelId!)
            .AsIChatClient();
    }

    private static void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }
}
