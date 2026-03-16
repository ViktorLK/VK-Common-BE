using Serilog;
using VK.Blocks.Observability.Serilog.Options;

namespace VK.Blocks.Observability.Serilog.Sinks;

/// <summary>
/// Configures the File sink with rolling intervals for Serilog.
/// </summary>
internal sealed class FileSinkConfigurator : ISinkConfigurator<SerilogOptions.FileOptions>
{
    public static LoggerConfiguration Configure(
        LoggerConfiguration loggerConfiguration,
        SerilogOptions.FileOptions options)
    {
        if (!options.Enabled)
            return loggerConfiguration;

        return loggerConfiguration.WriteTo.File(
            path: options.Path,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: options.RetainedFileCountLimit,
            outputTemplate: options.OutputTemplate);
    }
}
