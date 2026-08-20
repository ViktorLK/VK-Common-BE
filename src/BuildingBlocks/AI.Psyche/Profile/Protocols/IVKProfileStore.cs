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
    /// Retrieves a profile cognitive presence by profile ID.
    /// </summary>
    Task<VKResult<VKProfilePresence?>> GetProfileAsync(
        VKProfileId profileId,
        CancellationToken cancellationToken = default);
}
