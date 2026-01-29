namespace AIKit.Clients.Resilience;

/// <summary>
/// Settings for retry policy.
/// </summary>
public sealed class RetryPolicySettings
{
    /// <summary>
    /// Maximum number of retry attempts. Default is 3.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Initial delay between retries in seconds. Default is 1.
    /// </summary>
    public double InitialDelaySeconds { get; init; } = 1.0;

    /// <summary>
    /// Maximum delay between retries in seconds. Default is 60.
    /// </summary>
    public double MaxDelaySeconds { get; init; } = 60.0;

    /// <summary>
    /// Backoff multiplier for exponential backoff. Default is 2.0.
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;
}