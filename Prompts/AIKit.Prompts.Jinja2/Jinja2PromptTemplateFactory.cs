using Microsoft.SemanticKernel;
using System.Diagnostics.CodeAnalysis;

namespace AIKit.Prompts.Jinja2;

public sealed class Jinja2PromptTemplateFactory : IPromptTemplateFactory
{
    public bool TryCreate(
        PromptTemplateConfig templateConfig,
        [NotNullWhen(true)] out IPromptTemplate? result)
    {
        if (templateConfig == null)
        {
            result = null;
            return false;
        }

        // Only handle "jinja2" templates
        if (!string.Equals(templateConfig.TemplateFormat, "jinja2", StringComparison.OrdinalIgnoreCase))
        {
            result = null;
            return false;
        }

        result = new Jinja2PromptTemplate(templateConfig);
        return true;
    }
}