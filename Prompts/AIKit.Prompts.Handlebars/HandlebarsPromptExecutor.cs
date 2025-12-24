using AIKit.Core.Prompts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

namespace AIKit.Prompts.Handlebars;

public sealed class HandlebarsPromptExecutor : IPromptExecutor
{
    private readonly Kernel _kernel;
    private readonly HandlebarsPromptTemplateFactory _templateFactory;

    /// <summary>
    /// Create a new executor with a shared Kernel and optional custom chat client.
    /// </summary>
    public HandlebarsPromptExecutor(IChatClient chatClient)
        : this(BuildKernel(chatClient))
    {
    }

    /// <summary>
    /// Allows injecting an already built Kernel (for DI / reuse).
    /// </summary>
    public HandlebarsPromptExecutor(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _templateFactory = new HandlebarsPromptTemplateFactory();
    }

    private static Kernel BuildKernel(IChatClient chatClient)
    {
        if (chatClient == null) throw new ArgumentNullException(nameof(chatClient));

        var builder = Kernel.CreateBuilder();

        // Register the chat client as a chat completion service
        builder.Services.AddSingleton<IChatCompletionService>(chatClient.AsChatCompletionService());

        // Optional: also register the original IChatClient if needed later
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
}
