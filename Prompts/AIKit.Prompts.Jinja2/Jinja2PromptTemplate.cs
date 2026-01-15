using global::Jinja2.NET;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Environment = Jinja2.NET.Environment;

namespace AIKit.Prompts.Jinja2;


internal sealed class Jinja2PromptTemplate : IPromptTemplate
{
    private readonly PromptTemplateConfig _config;
    private readonly Template _template;
    private readonly ILogger<Jinja2PromptTemplate>? _logger;

    public Jinja2PromptTemplate(PromptTemplateConfig config, ILogger<Jinja2PromptTemplate>? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger;

        // Create an isolated Jinja environment
        var environment = new Environment
        {
        };

        try
        {
            // Parse template (real Jinja2 syntax)
            _template = environment.FromString(_config.Template);
            _logger?.LogInformation("Initialized Jinja2 prompt template");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to parse Jinja2 template");
            throw new ArgumentException("Invalid Jinja2 template syntax.", nameof(config.Template), ex);
        }
    }

    public Task<string> RenderAsync(
        Kernel kernel,
        KernelArguments arguments,
        CancellationToken cancellationToken = default)
    {
        // Jinja context
        var context = new Dictionary<string, object?>();

        // Expose KernelArguments
        foreach (var kv in arguments)
        {
            context[kv.Key] = kv.Value;
        }

        // Optional: expose kernel metadata
        context["kernel"] = kernel;

        try
        {
            var result = _template.Render(context);
            _logger?.LogDebug("Rendered Jinja2 template with {ArgumentCount} arguments", arguments.Count);
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to render Jinja2 template");
            throw new InvalidOperationException("Template rendering failed due to variable substitution error or invalid syntax.", ex);
        }
    }
}
