using Serilog.Core;
using Serilog.Events;
using Microsoft.Extensions.Hosting;

namespace VK.Blocks.Observability.Serilog.Enrichers;

/// <summary>
/// Enriches log events with application metadata such as Name, Version, and Environment.
/// </summary>
public sealed class ApplicationEnricher(IHostEnvironment environment) : ILogEventEnricher
{
    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(SerilogPropertyNames.ApplicationName, environment.ApplicationName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(SerilogPropertyNames.Environment, environment.EnvironmentName));

        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            var version = entryAssembly.GetName().Version?.ToString();
            if (!string.IsNullOrEmpty(version))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(SerilogPropertyNames.Version, version));
            }
        }
    }
}
