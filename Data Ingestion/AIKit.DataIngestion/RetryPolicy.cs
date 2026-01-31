namespace AIKit.DataIngestion;

/// <summary>
/// Represents a retry policy for pipeline execution.
/// </summary>
public class RetryPolicy
{
    /// <summary>
    /// Gets or sets the maximum number of retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retries.
    /// </summary>
    public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);
}