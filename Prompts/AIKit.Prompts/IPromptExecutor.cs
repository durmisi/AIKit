using Microsoft.SemanticKernel;

namespace AIKit.Prompts;

/// <summary>
/// Executes prompt templates of various types.
/// </summary>
public interface IPromptExecutor
{
    /// <summary>
    /// Gets the template type this executor supports (e.g. "handlebars", "semantic-kernel", "liquid").
    /// </summary>
    string TemplateType { get; }

    /// <summary>
    /// Executes the prompt template and returns the full result as a string.
    /// </summary>
    Task<string> ExecuteAsync(
        string template,
        KernelArguments arguments,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the prompt template in streaming mode, yielding results as they are generated.
    /// </summary>
    IAsyncEnumerable<string> ExecuteStreamingAsync(
        string template,
        KernelArguments arguments,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default);
}
