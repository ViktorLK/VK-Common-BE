using Serilog;

namespace VK.Blocks.Observability.Serilog.Sinks;

/// <summary>
/// Defines an interface for configuring Serilog sinks.
/// </summary>
internal interface ISinkConfigurator<in TOptions>
{
    /// <summary>
    /// Configures the specified sink onto the logger configuration.
    /// </summary>
    /// <param name="loggerConfiguration">The current logger configuration.</param>
    /// <param name="options">The options for the sink.</param>
    /// <returns>The updated logger configuration.</returns>
    static abstract LoggerConfiguration Configure(LoggerConfiguration loggerConfiguration, TOptions options);
}
