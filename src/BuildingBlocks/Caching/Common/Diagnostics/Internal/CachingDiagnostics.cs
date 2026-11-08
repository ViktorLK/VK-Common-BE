using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Partial class for caching diagnostics implementation.
/// Follows OR.01, BB.04.
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

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Cache operation '{Operation}' failed for key '{Key}'.")]
    internal static partial void LogCacheOperationFailed(this ILogger logger, Exception ex, string operation, string key);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Cache miss or entry unavailable for key '{Key}'.")]
    internal static partial void LogCacheMiss(this ILogger logger, string key);
}
