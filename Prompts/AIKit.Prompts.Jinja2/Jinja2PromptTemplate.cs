using global::Jinja2.NET;
using Microsoft.SemanticKernel;
using Environment = Jinja2.NET.Environment;

namespace AIKit.Prompts.Jinja2;


internal sealed class Jinja2PromptTemplate : IPromptTemplate
{
    private readonly PromptTemplateConfig _config;
    private readonly Template _template;

    public Jinja2PromptTemplate(PromptTemplateConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        // Create an isolated Jinja environment
        var environment = new Environment
        {
        };

        // Parse template (real Jinja2 syntax)
        _template = environment.FromString(_config.Template);
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

        var result = _template.Render(context);
        return Task.FromResult(result);
    }
}
