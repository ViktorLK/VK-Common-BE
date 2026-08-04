using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to manage user cognitive presence.
/// Follows CS.01, CS.03, and Ambient Context isolation patterns.
/// </summary>
public interface IVKUserStore
{
    /// <summary>
    /// Retrieves a user cognitive presence by user ID within current ambient identity.
    /// </summary>
    Task<VKResult<VKUserPresence?>> GetPresenceAsync(
        VKUserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates a user cognitive presence.
    /// </summary>
    Task<VKResult> SavePresenceAsync(
        VKUserPresence presence,
        CancellationToken cancellationToken = default);
}
