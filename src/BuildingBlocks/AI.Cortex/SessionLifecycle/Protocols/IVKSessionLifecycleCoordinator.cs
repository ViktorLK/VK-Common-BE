using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex;

/// <summary>
/// Cross-block lifecycle coordinator for detecting session boundaries (idle/day change)
/// and sequentially triggering downstream post-session consolidations (Engram, Somatic, Corpus).
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKSessionLifecycleCoordinator
{
    /// <summary>
    /// Pure function evaluation: evaluates whether a session is expired based on the last activity timestamp against configured thresholds.
    /// </summary>
    /// <param name="lastActivityAt">The last active timestamp provided by the App layer.</param>
    /// <returns><c>true</c> if the session is considered expired; otherwise, <c>false</c>.</returns>
    bool IsSessionExpired(DateTimeOffset lastActivityAt);

    /// <summary>
    /// Sequentially dispatches session termination triggers across Engram, Somatic, and Corpus.
    /// </summary>
    Task<VKResult> OnSessionEndedAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default);
}
