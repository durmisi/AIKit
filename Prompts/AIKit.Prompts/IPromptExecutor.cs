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

/// <summary>
/// Manages prompt executors for different template types.
/// </summary>
public sealed class PromptExecutor
{
    private readonly IReadOnlyDictionary<string, IPromptExecutor> _executors;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptExecutor"/> class.
    /// </summary>
    /// <param name="executors">The collection of prompt executors.</param>
    public PromptExecutor(IEnumerable<IPromptExecutor> executors)
    {
        ArgumentNullException.ThrowIfNull(executors);

        // Build lookup once – cheap & fast at runtime
        _executors = executors.ToDictionary(
            e => e.TemplateType,
            StringComparer.OrdinalIgnoreCase);
    }

    private IPromptExecutor Resolve(string templateType)
    {
        if (string.IsNullOrWhiteSpace(templateType))
            throw new ArgumentException("Template type must be provided.", nameof(templateType));

        if (_executors.TryGetValue(templateType, out var executor))
            return executor;

        throw new NotSupportedException(
            $"No IPromptExecutor registered for template type '{templateType}'.");
    }

    public Task<string> ExecuteAsync(
        string templateType,
        string template,
        KernelArguments arguments,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        var executor = Resolve(templateType);

        return executor.ExecuteAsync(
            template,
            arguments,
            executionSettings,
            cancellationToken);
    }

    public IAsyncEnumerable<string> ExecuteStreamingAsync(
        string templateType,
        string template,
        KernelArguments arguments,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        var executor = Resolve(templateType);

        return executor.ExecuteStreamingAsync(
            template,
            arguments,
            executionSettings,
            cancellationToken);
    }
}
