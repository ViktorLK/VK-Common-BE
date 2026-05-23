using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Defines the contract for persisting and resuming situational presence states across stateless requests.
/// Follows CS.01 (Result Pattern), CS.03 (CancellationToken support), and AP.03 (public visibility, VK prefix).
/// </summary>
public interface IVKPresenceStateStore
{
    /// <summary>
    /// Persists the active presence state snapshot for a session.
    /// </summary>
    /// <param name="key">The presence store composite key.</param>
    /// <param name="state">The presence state snapshot.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating operation status.</returns>
    Task<VKResult> SaveStateAsync(
        VKPresenceStoreKey key,
        VKPresenceState state,
        CancellationToken cancellationToken = default); // [CS.03]

    /// <summary>
    /// Loads a previously persisted presence state snapshot for a session.
    /// </summary>
    /// <param name="key">The presence store composite key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the presence state if found.</returns>
    Task<VKResult<VKPresenceState>> LoadStateAsync(
        VKPresenceStoreKey key,
        CancellationToken cancellationToken = default); // [CS.03]
}
