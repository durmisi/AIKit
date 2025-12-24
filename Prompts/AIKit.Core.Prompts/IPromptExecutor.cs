using Microsoft.SemanticKernel;

namespace AIKit.Core.Prompts;
public interface IPromptExecutor
{
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
