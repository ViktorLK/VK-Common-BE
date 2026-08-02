using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Service contract for prefetching relevant memories based on predictive cues.
/// </summary>
public interface IVKPredictiveMemoryPrefetcher
{
    /// <summary>
    /// Asynchronously prefetches relevant L3 memories into context cache using extracted predictive cues.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKMemoryEntry>>> PrefetchContextAsync(
        string predictiveCue,
        string? queryCue = null,
        VKTenantId? tenantId = null,
        int topK = 5,
        CancellationToken cancellationToken = default);
}
