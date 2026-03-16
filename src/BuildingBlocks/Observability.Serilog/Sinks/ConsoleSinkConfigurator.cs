using Serilog;
using VK.Blocks.Observability.Serilog.Options;

namespace VK.Blocks.Observability.Serilog.Sinks;

/// <summary>
/// Configures the Console sink for Serilog.
/// </summary>
internal sealed class ConsoleSinkConfigurator : ISinkConfigurator<SerilogOptions.ConsoleOptions>
{
    public static LoggerConfiguration Configure(
        LoggerConfiguration loggerConfiguration,
        SerilogOptions.ConsoleOptions options)
    {
        if (!options.Enabled)
            return loggerConfiguration;

        return loggerConfiguration.WriteTo.Console(
            outputTemplate: options.OutputTemplate);
    }
}
