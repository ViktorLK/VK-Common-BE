using System.Diagnostics;
using VK.Blocks.Observability.Conventions;

namespace VK.Blocks.Observability.Enrichment;

/// <summary>
/// An enricher that adds the current trace context (TraceId, SpanId) to the log context.
/// Only adds properties if an active <see cref="Activity"/> exists.
/// </summary>
public sealed class TraceContextEnricher : ILogEnricher
{
    #region Public Methods

    /// <inheritdoc />
    public void Enrich(Action<string, object?> propertyAdder)
    {
        var activity = Activity.Current;
        if (activity is not null)
        {
            propertyAdder(FieldNames.TraceId, activity.TraceId.ToHexString());
            propertyAdder(FieldNames.SpanId, activity.SpanId.ToHexString());
        }
    }

    #endregion
}
