using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VK.Blocks.Observability;

public sealed class VKActivityLogContextEnricher(IEnumerable<IVKLogEnricher> logEnrichers) : IVKLogContextEnricher
{
    public IDisposable Enrich()
    {
        var activity = Activity.Current;
        if (activity == null)
        {
            return NullScope.Instance;
        }

        var properties = new Dictionary<string, object?>();
        foreach (var enricher in logEnrichers)
        {
            enricher.Enrich((k, v) => properties[k] = v);
        }

        return new LogScope(properties);
    }

    private sealed class LogScope(Dictionary<string, object?> properties) : IDisposable
    {
        public void Dispose()
        {
            properties.Clear();
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}
