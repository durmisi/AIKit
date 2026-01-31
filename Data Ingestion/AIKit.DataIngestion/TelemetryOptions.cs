/// <summary>
/// Options for configuring telemetry in the ingestion pipeline.
/// </summary>
public class TelemetryOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether telemetry is enabled. Default is false.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the service name for telemetry resources. Default is "AIKit.DataIngestion".
    /// </summary>
    public string ServiceName { get; set; } = "AIKit.DataIngestion";
}