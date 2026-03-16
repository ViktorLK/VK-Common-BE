using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace VK.Blocks.Observability.Serilog.Enrichers;

/// <summary>
/// Enriches log events with TraceId and SpanId from the current Activity.
/// </summary>
public sealed class TraceContextEnricher : ILogEventEnricher
{
    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity == null)
            return;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(SerilogPropertyNames.TraceId, activity.TraceId.ToHexString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(SerilogPropertyNames.SpanId, activity.SpanId.ToHexString()));

        if (activity.ParentSpanId != default)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(SerilogPropertyNames.ParentId, activity.ParentSpanId.ToHexString()));
        }
    }
}
