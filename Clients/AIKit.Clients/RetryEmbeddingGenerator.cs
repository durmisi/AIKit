using Microsoft.Extensions.AI;

namespace AIKit.Clients;

/// <summary>
/// An embedding generator that wraps another generator with retry logic.
/// </summary>
public sealed class RetryEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _innerGenerator;
    private readonly RetryPolicySettings _retrySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryEmbeddingGenerator"/> class.
    /// </summary>
    /// <param name="innerGenerator">The inner embedding generator to wrap.</param>
    /// <param name="retrySettings">The retry policy settings.</param>
    public RetryEmbeddingGenerator(IEmbeddingGenerator<string, Embedding<float>> innerGenerator, RetryPolicySettings retrySettings)
    {
        _innerGenerator = innerGenerator ?? throw new ArgumentNullException(nameof(innerGenerator));
        _retrySettings = retrySettings ?? throw new ArgumentNullException(nameof(retrySettings));
    }

    /// <inheritdoc />
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var exceptions = new List<Exception>();
        var delay = TimeSpan.FromSeconds(_retrySettings.InitialDelaySeconds);

        for (int attempt = 0; attempt <= _retrySettings.MaxRetries; attempt++)
        {
            try
            {
                return await _innerGenerator.GenerateAsync(values, options, cancellationToken);
            }
            catch (Exception ex) when (IsTransientException(ex))
            {
                exceptions.Add(ex);
                if (attempt < _retrySettings.MaxRetries)
                {
                    await Task.Delay(delay, cancellationToken);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * _retrySettings.BackoffMultiplier, _retrySettings.MaxDelaySeconds));
                }
            }
        }

        throw new AggregateException($"Embedding generation failed after {_retrySettings.MaxRetries + 1} attempts. See inner exceptions for details.", exceptions);
    }

    /// <summary>
    /// Determines if an exception is transient and should be retried.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if the exception is transient; otherwise, false.</returns>
    private static bool IsTransientException(Exception ex)
    {
        // Consider network, timeout, rate limit, and server errors as transient
        return ex is HttpRequestException ||
               ex is TimeoutException ||
               (ex is InvalidOperationException && ex.Message.Contains("rate limit")) ||
               (ex is InvalidOperationException && ex.Message.Contains("429")) ||
               (ex is InvalidOperationException && ex.Message.Contains("502")) ||
               (ex is InvalidOperationException && ex.Message.Contains("503")) ||
               (ex is InvalidOperationException && ex.Message.Contains("504"));
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null) => _innerGenerator.GetService(serviceType, serviceKey);

    /// <inheritdoc />
    public void Dispose() => _innerGenerator.Dispose();
}