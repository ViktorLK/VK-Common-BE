using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Partial class for caching diagnostics implementation.
/// </summary>
[VKBlockDiagnostics<VKCachingBlock>]
internal static partial class CachingDiagnostics
{
    private static readonly Counter<long> _cacheHits;
    private static readonly Counter<long> _cacheMisses;
    private static readonly Counter<long> _cacheErrors;

    static CachingDiagnostics()
    {
        _cacheHits = Meter.CreateCounter<long>("cache.hits", "count", "Total number of cache hits");
        _cacheMisses = Meter.CreateCounter<long>("cache.misses", "count", "Total number of cache misses");
        _cacheErrors = Meter.CreateCounter<long>("cache.errors", "count", "Total number of cache errors");
    }

    internal static Activity? StartActivity(string name) => Source.StartActivity(name);

    internal static void RecordHit() => _cacheHits.Add(1);

    internal static void RecordMiss() => _cacheMisses.Add(1);

    internal static void RecordError(string operation)
    {
        _cacheErrors.Add(1, new TagList { { "operation", operation } });
    }
}
