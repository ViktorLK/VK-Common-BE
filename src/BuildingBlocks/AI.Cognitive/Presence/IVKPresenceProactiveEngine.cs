using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Delegate invoked by the proactive engine when inactivity triggers a heartbeat pulse.
/// </summary>
/// <param name="sessionId">The session identifier.</param>
/// <param name="personaId">The persona identifier.</param>
/// <param name="cancellationToken">The cancellation token.</param>
public delegate Task PresenceProactiveCallback(
    string sessionId,
    string personaId,
    CancellationToken cancellationToken);

/// <summary>
/// Handles background inactivity monitoring and triggers proactive interaction.
/// </summary>
public interface IVKPresenceProactiveEngine
{
    /// <summary>
    /// Registers the callback delegate invoked on proactive inactivity trigger.
    /// </summary>
    /// <param name="callback">The callback delegate.</param>
    void RegisterCallback(PresenceProactiveCallback callback);

    /// <summary>
    /// Notifies the proactive engine of user activity, resetting the inactivity timer.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="personaId">The persona identifier.</param>
    void NotifyInteraction(string sessionId, string personaId);

    /// <summary>
    /// Force triggers a proactive pulse.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="personaId">The persona identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating trigger status.</returns>
    Task<VKResult> PulseAsync(
        string sessionId,
        string personaId,
        CancellationToken cancellationToken = default); // [CS.03]
}
