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

    /// <summary>
    /// Resolves the prompt executor for the specified template type.
    /// </summary>
    /// <param name="templateType">The template type to resolve.</param>
    /// <returns>The <see cref="IPromptExecutor"/> for the template type.</returns>
    /// <exception cref="ArgumentException">Thrown if templateType is null or whitespace.</exception>
    /// <exception cref="NotSupportedException">Thrown if no executor is registered for the template type.</exception>
    private IPromptExecutor Resolve(string templateType)
    {
        if (string.IsNullOrWhiteSpace(templateType))
            throw new ArgumentException("Template type must be provided.", nameof(templateType));

        if (_executors.TryGetValue(templateType, out var executor))
            return executor;

        throw new NotSupportedException(
            $"No IPromptExecutor registered for template type '{templateType}'.");
    }

    /// <summary>
    /// Executes the prompt template for the specified template type and returns the full result as a string.
    /// </summary>
    /// <param name="templateType">The type of the template (e.g., "handlebars").</param>
    /// <param name="template">The template string to execute.</param>
    /// <param name="arguments">The arguments to pass to the template.</param>
    /// <param name="executionSettings">Optional execution settings.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The executed template result as a string.</returns>
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

    /// <summary>
    /// Executes the prompt template for the specified template type in streaming mode, yielding results as they are generated.
    /// </summary>
    /// <param name="templateType">The type of the template (e.g., "handlebars").</param>
    /// <param name="template">The template string to execute.</param>
    /// <param name="arguments">The arguments to pass to the template.</param>
    /// <param name="executionSettings">Optional execution settings.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An async enumerable of string results.</returns>
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
