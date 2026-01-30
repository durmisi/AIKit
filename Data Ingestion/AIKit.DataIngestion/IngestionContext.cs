namespace AIKit.DataIngestion;

/// <summary>
/// Base class for ingestion contexts that provides error tracking.
/// </summary>
public class IngestionContext
{
    /// <summary>
    /// Gets the list of errors encountered during processing.
    /// </summary>
    public List<string> Errors { get; } = new();
}