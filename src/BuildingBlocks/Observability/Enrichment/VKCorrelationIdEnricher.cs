using System;
using System.Diagnostics;
using VK.Blocks.Core;

namespace VK.Blocks.Observability;

/// <summary>
/// Enriches log events with the correlation ID derived from the active trace context or baggage.
/// Complies with Manifest §6 (Automatic Cross-Signal Correlation).
/// </summary>
// [AP.01] sealed
// [AP.03] Level 1 Public API with VK prefix
public sealed class VKCorrelationIdEnricher : IVKLogEnricher
{
    public void Enrich(Action<string, object?> propertyAdder)
    {
        VKGuard.NotNull(propertyAdder, nameof(propertyAdder));

        var activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        var correlationId = activity.GetBaggageItem(FieldNames.CorrelationId)
            ?? activity.GetBaggageItem("correlation.id")
            ?? (activity.TraceId != default ? activity.TraceId.ToHexString() : null);

        if (!string.IsNullOrEmpty(correlationId))
        {
            propertyAdder(FieldNames.CorrelationId, correlationId);
            propertyAdder("correlation.id", correlationId);
        }
    }
}
