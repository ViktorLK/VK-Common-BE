using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core.Attributes;

namespace VK.Blocks.Caching.Diagnostics;

/// <summary>
/// Partial class for caching diagnostics implementation.
/// </summary>
[VKBlockDiagnostics("VK.Blocks.Caching")]
internal static partial class CachingDiagnostics
{
    private static readonly ActivitySource ActivitySource = new("VK.Blocks.Caching");
    private static readonly Meter _meter = new("VK.Blocks.Caching");

    public static readonly Counter<long> CacheHits = _meter.CreateCounter<long>("cache.hits", "count", "Total number of cache hits");
    public static readonly Counter<long> CacheMisses = _meter.CreateCounter<long>("cache.misses", "count", "Total number of cache misses");
    public static readonly Counter<long> CacheErrors = _meter.CreateCounter<long>("cache.errors", "count", "Total number of cache errors");

    public static Activity? StartActivity(string name) => ActivitySource.StartActivity(name);
}
