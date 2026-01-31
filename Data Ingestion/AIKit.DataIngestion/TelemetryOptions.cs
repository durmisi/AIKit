/// <summary>
/// Options for configuring telemetry in the ingestion pipeline.
/// </summary>
public class TelemetryOptions
{
    /// <summary>
    /// Gets or sets the name of the activity source for tracing. Default is "AIKit.DataIngestion".
    /// </summary>
    public string ActivitySourceName { get; set; } = "AIKit.DataIngestion";

    /// <summary>
    /// Gets or sets the name of the meter for metrics. Default is "AIKit.DataIngestion".
    /// </summary>
    public string MeterName { get; set; } = "AIKit.DataIngestion";

    /// <summary>
    /// Gets or sets a value indicating whether telemetry is enabled. Default is false.
    /// </summary>
    public bool Enabled { get; set; } = false;
}