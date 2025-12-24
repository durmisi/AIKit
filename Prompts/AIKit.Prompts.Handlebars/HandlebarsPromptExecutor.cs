using AIKit.Core.Prompts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.Runtime.CompilerServices;

namespace AIKit.Prompts.Handlebars;

public sealed class HandlebarsPromptExecutor : IPromptExecutor
{
    private readonly Kernel _kernel;
    private readonly HandlebarsPromptTemplateFactory _templateFactory;

    public HandlebarsPromptExecutor(IChatClient chatClient)
        : this(BuildKernel(chatClient))
    {
    }

    public HandlebarsPromptExecutor(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _templateFactory = new HandlebarsPromptTemplateFactory();
    }

    private static Kernel BuildKernel(IChatClient chatClient)
    {
        if (chatClient == null) throw new ArgumentNullException(nameof(chatClient));

        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton<IChatCompletionService>(chatClient.AsChatCompletionService());
        builder.Services.AddSingleton<IChatClient>(chatClient);

        return builder.Build();
    }

    public async Task<string> ExecuteAsync(
        string template,
        KernelArguments arguments,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Prompt template cannot be empty", nameof(template));

        var config = new PromptTemplateConfig
        {
            Name = "HandlebarsPrompt",
            Template = template,
            TemplateFormat = "handlebars",
        };

        var settings = executionSettings ?? new PromptExecutionSettings
        {
            ServiceId = PromptExecutionSettings.DefaultServiceId,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };

        config.AddExecutionSettings(settings);

        var function = _kernel.CreateFunctionFromPrompt(config, _templateFactory);

        var result = await _kernel.InvokeAsync(function, arguments, cancellationToken)
                                  .ConfigureAwait(false);

        return result.GetValue<string>() ?? string.Empty;
    }

    /// <summary>
    /// Executes the prompt in streaming mode, yielding tokens as they arrive.
    /// </summary>
    public async IAsyncEnumerable<string> ExecuteStreamingAsync(
        string template,
        KernelArguments arguments,
        PromptExecutionSettings? executionSettings = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Prompt template cannot be empty", nameof(template));

        var config = new PromptTemplateConfig
        {
            Name = "HandlebarsPrompt",
            Template = template,
            TemplateFormat = "handlebars",
        };

        var settings = executionSettings ?? new PromptExecutionSettings
        {
            ServiceId = PromptExecutionSettings.DefaultServiceId,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
        };

        config.AddExecutionSettings(settings);

        var function = _kernel.CreateFunctionFromPrompt(config, _templateFactory);

        await foreach (var result in _kernel.InvokeStreamingAsync(function, arguments, cancellationToken)
                                         .ConfigureAwait(false))
        {
            if (result is Microsoft.SemanticKernel.StreamingChatMessageContent chatContent)
            {
                var text = chatContent.Content;

                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
    }
}
