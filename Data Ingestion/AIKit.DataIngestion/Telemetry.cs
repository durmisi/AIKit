using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace AIKit.DataIngestion;

/// <summary>
/// Provides telemetry sources for AIKit.DataIngestion.
/// Applications should add these to their TracerProvider and MeterProvider.
/// </summary>
public static class Telemetry
{
    /// <summary>
    /// ActivitySource for tracing ingestion pipeline operations.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new("AIKit.DataIngestion");

    /// <summary>
    /// Meter for metrics related to ingestion pipeline operations.
    /// </summary>
    public static readonly Meter Meter = new("AIKit.DataIngestion");
}