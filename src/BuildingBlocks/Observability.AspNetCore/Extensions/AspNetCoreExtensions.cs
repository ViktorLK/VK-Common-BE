using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace VK.Blocks.Observability.AspNetCore.Extensions;

/// <summary>
/// Provides extension methods for configuring OpenTelemetry instrumentation for ASP.NET Core.
/// </summary>
public static class AspNetCoreExtensions
{
    /// <summary>
    /// Adds ASP.NET Core and HttpClient instrumentation to the tracer provider.
    /// </summary>
    /// <param name="builder">The <see cref="TracerProviderBuilder"/>.</param>
    /// <returns>The updated <see cref="TracerProviderBuilder"/>.</returns>
    public static TracerProviderBuilder AddVKAspNetCoreInstrumentation(this TracerProviderBuilder builder)
    {
        return builder.AddAspNetCoreInstrumentation()
                      .AddHttpClientInstrumentation();
    }

    /// <summary>
    /// Adds ASP.NET Core instrumentation to the meter provider.
    /// </summary>
    /// <param name="builder">The <see cref="MeterProviderBuilder"/>.</param>
    /// <returns>The updated <see cref="MeterProviderBuilder"/>.</returns>
    public static MeterProviderBuilder AddVKAspNetCoreInstrumentation(this MeterProviderBuilder builder)
    {
        return builder.AddAspNetCoreInstrumentation();
    }
}
