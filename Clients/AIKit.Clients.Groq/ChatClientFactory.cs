using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace AIKit.Clients.Groq;

public sealed class ChatClientFactory : BaseChatClientFactory
{
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
        : base(settings, logger)
    {
    }

    protected override string GetDefaultProviderName() => "groq";

    protected override void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireApiKey(settings);
        AIClientSettingsValidator.RequireModel(settings);
    }

    protected override IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri("https://api.groq.com/openai/v1/")
        };

        if (settings.HttpClient != null)
        {
            options.Transport = new HttpClientPipelineTransport(settings.HttpClient);
        }
        else
        {
            options.NetworkTimeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        }

        var credential = new ApiKeyCredential(settings.ApiKey!);
        var client = new OpenAIClient(credential, options);

        var targetModel = modelName ?? settings.ModelId!;
        _logger?.LogInformation("Creating Groq chat client for model {Model}", targetModel);

        return client.GetChatClient(targetModel).AsIChatClient();
    }
}