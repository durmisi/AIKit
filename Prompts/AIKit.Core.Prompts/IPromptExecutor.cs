using Microsoft.SemanticKernel;

namespace AIKit.Core.Prompts;

public interface IPromptExecutor
{
    Task<string> ExecuteAsync(
        string template,
        KernelArguments arguments,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default);
}