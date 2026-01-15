using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AIKit.Prompts.Jinja2;

public sealed class Jinja2PromptExecutor : IPromptExecutor
{
    private readonly Kernel _kernel;
    private readonly Jinja2PromptTemplateFactory _templateFactory;

    // Security limits
    private const int MaxTemplateLength = 50000;
    private const int MaxArgumentsCount = 100;
    private const int MaxArgumentKeyLength = 100;
    private const int MaxArgumentValueLength = 10000;

    public string TemplateType => "jinja2";

    public Jinja2PromptExecutor(IChatClient chatClient)
        : this(BuildKernel(chatClient))
    {
    }

    public Jinja2PromptExecutor(Kernel kernel)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _templateFactory = new Jinja2PromptTemplateFactory();
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
        ValidateInputs(template, arguments);

        var config = new PromptTemplateConfig
        {
            Name = "Jinja2Prompt",
            Template = template,
            TemplateFormat = TemplateType,
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
        ValidateInputs(template, arguments);

        var config = new PromptTemplateConfig
        {
            Name = "Jinja2Prompt",
            Template = template,
            TemplateFormat = TemplateType,
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

    /// <summary>
    /// Validates inputs to prevent prompt injection attacks.
    /// </summary>
    private static void ValidateInputs(string template, KernelArguments arguments)
    {
        // Validate template
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Prompt template cannot be empty", nameof(template));

        if (template.Length > MaxTemplateLength)
            throw new ArgumentException($"Template length exceeds maximum allowed ({MaxTemplateLength} characters)", nameof(template));

        // Check for suspicious patterns in template
        if (ContainsSuspiciousPatterns(template))
            throw new ArgumentException("Template contains potentially unsafe patterns", nameof(template));

        // Validate arguments
        if (arguments == null)
            throw new ArgumentNullException(nameof(arguments));

        if (arguments.Count > MaxArgumentsCount)
            throw new ArgumentException($"Too many arguments (maximum {MaxArgumentsCount} allowed)", nameof(arguments));

        foreach (var key in arguments.Keys)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Argument key cannot be empty", nameof(arguments));

            if (key.Length > MaxArgumentKeyLength)
                throw new ArgumentException($"Argument key too long (maximum {MaxArgumentKeyLength} characters)", nameof(key));

            // Validate key format (alphanumeric, underscore, dash only)
            if (!Regex.IsMatch(key, @"^[a-zA-Z_][a-zA-Z0-9_-]*$"))
                throw new ArgumentException($"Invalid argument key format: {key}", nameof(arguments));

            var value = arguments[key];
            if (value != null)
            {
                var stringValue = value.ToString();
                if (stringValue != null && stringValue.Length > MaxArgumentValueLength)
                    throw new ArgumentException($"Argument value too long for key '{key}' (maximum {MaxArgumentValueLength} characters)", nameof(arguments));
            }
        }
    }

    /// <summary>
    /// Checks for potentially unsafe patterns in templates.
    /// </summary>
    private static bool ContainsSuspiciousPatterns(string template)
    {
        // Patterns that might indicate injection attempts
        var suspiciousPatterns = new[]
        {
            @"\{\{\s*\$",           // Dollar signs in variables
            @"\{\%\s*set\s+.*=.*import", // Import statements in Jinja2
            @"\{\%\s*set\s+.*=.*exec",   // Exec in assignments
            @"\{\%\s*set\s+.*=.*eval",   // Eval in assignments
            @"env\s*\.",            // Environment variable access
            @"global\s*\.",         // Global object access
            @"window\s*\.",         // Browser window access
            @"document\s*\.",       // DOM access
            @"process\s*\.",        // Node.js process access
            @"require\s*\(",        // Module require
            @"import\s+",           // Import statements
            @"eval\s*\(",           // Eval function
            @"exec\s*\(",           // Exec function
            @"system\s*\(",         // System calls
            @"__import__\s*\(",     // Python import
            @"__builtins__",       // Python builtins
        };

        foreach (var pattern in suspiciousPatterns)
        {
            if (Regex.IsMatch(template, pattern, RegexOptions.IgnoreCase))
                return true;
        }

        return false;
    }
}
