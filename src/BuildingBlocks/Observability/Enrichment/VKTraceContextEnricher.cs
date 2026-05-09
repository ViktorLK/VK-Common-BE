using System;
using System.Diagnostics;
namespace VK.Blocks.Observability;

public sealed class VKTraceContextEnricher : IVKLogEnricher
{
    public void Enrich(Action<string, object?> propertyAdder)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            propertyAdder("trace.id", activity.TraceId.ToHexString());
            propertyAdder("span.id", activity.SpanId.ToHexString());
        }
    }
}
