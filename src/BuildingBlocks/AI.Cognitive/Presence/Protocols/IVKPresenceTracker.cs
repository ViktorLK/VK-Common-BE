using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Manages situational and operational presence (Working Memory, Context Window, and session metadata).
/// Follows CS.01, CS.03, and AP.01.
/// </summary>
public interface IVKPresenceTracker
{
    /// <summary>
    /// Computes the current situational and operational presence state for a session.
    /// </summary>
    /// <param name="sessionId">The active session ID.</param>
    /// <param name="input">The latest user input.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the computed presence state.</returns>
    Task<VKResult<VKPresenceState>> CaptureStateAsync(
        string sessionId,
        string? input,
        VKWorldState? worldState = null,
        CancellationToken cancellationToken = default); // [CS.03]

    /// <summary>
    /// Records token usage for a session to update active memory budgets.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="promptTokens">Tokens used for prompt.</param>
    /// <param name="completionTokens">Tokens used for completion.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating operation status.</returns>
    Task<VKResult> RecordUsageAsync(
        string sessionId,
        int promptTokens,
        int completionTokens,
        CancellationToken cancellationToken = default); // [CS.03]

    /// <summary>
    /// Retrieves the active presence state of a session.
    /// </summary>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the current presence state.</returns>
    Task<VKResult<VKPresenceState>> GetStateAsync(
        string sessionId,
        CancellationToken cancellationToken = default); // [CS.03]
}
