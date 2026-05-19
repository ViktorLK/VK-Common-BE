using System.Collections.Concurrent;
using System.Threading;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Provides session-level concurrency synchronization to prevent collisions
/// between reactive user prompts and background proactive heartbeats.
/// </summary>
// // [AP.01] Static classes are implicitly sealed by C# compiler
public static class VKSessionLock
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new(System.StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or creates a thread-safe semaphore for the specified session identifier.
    /// </summary>
    /// <param name="sessionId">The unique session identifier.</param>
    /// <returns>A SemaphoreSlim instance dedicated to the session.</returns>
    public static SemaphoreSlim GetLock(string sessionId)
    {
        VKGuard.NotNullOrWhiteSpace(sessionId); // [AP.01] Defensive check
        return Locks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
    }
}
