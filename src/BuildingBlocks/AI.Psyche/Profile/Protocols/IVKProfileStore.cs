using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to manage profile cognitive presence.
/// Follows CS.01, CS.03, and Ambient Context isolation patterns.
/// </summary>
public interface IVKProfileStore
{
    /// <summary>
    /// Retrieves a profile cognitive presence by user ID within current ambient identity.
    /// </summary>
    Task<VKResult<VKProfilePresence?>> GetProfileAsync(
        VKUserId userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates a profile cognitive presence.
    /// </summary>
    Task<VKResult> SaveProfileAsync(
        VKProfilePresence presence,
        CancellationToken cancellationToken = default);
}
