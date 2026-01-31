using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace AIKit.DataIngestion;

/// <summary>
/// Extension methods for configuring OpenTelemetry providers with AIKit.DataIngestion telemetry.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Adds AIKit.DataIngestion tracing to the OpenTelemetry configuration.
    /// </summary>
    /// <param name="builder">The TracerProviderBuilder to add tracing.</param>
    /// <returns>The updated TracerProviderBuilder.</returns>
    public static TracerProviderBuilder AddAIKitTracing(this TracerProviderBuilder builder)
    {
        return builder.AddSource(Telemetry.ActivitySource.Name);
    }

    /// <summary>
    /// Adds AIKit.DataIngestion metrics to the OpenTelemetry configuration.
    /// </summary>
    /// <param name="builder">The MeterProviderBuilder to add metrics.</param>
    /// <returns>The updated MeterProviderBuilder.</returns>
    public static MeterProviderBuilder AddAIKitMetrics(this MeterProviderBuilder builder)
    {
        return builder.AddMeter(Telemetry.Meter.Name);
    }
}