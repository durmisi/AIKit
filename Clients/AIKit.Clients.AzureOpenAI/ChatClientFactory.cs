using AIKit.Clients.Base;
using AIKit.Clients.Settings;
using Azure;
using Azure.AI.Inference;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AIKit.Clients.AzureOpenAI;

public sealed class ChatClientFactory : BaseChatClientFactory
{
    public ChatClientFactory(AIClientSettings settings, ILogger<ChatClientFactory>? logger = null)
        : base(settings, logger)
    {
    }

    protected override string GetDefaultProviderName() => "azure-open-ai";

    protected override void Validate(AIClientSettings settings)
    {
        AIClientSettingsValidator.RequireEndpoint(settings);
        AIClientSettingsValidator.RequireModel(settings);

        if (!settings.UseDefaultAzureCredential)
        {
            AIClientSettingsValidator.RequireApiKey(settings);
        }
    }

    protected override IChatClient CreateClient(AIClientSettings settings, string? modelName)
    {
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

        var targetModel = modelName ?? settings.ModelId;
        _logger?.LogInformation("Creating Azure OpenAI chat client for model {Model}", targetModel);

        return client.AsIChatClient(targetModel);
    }
}