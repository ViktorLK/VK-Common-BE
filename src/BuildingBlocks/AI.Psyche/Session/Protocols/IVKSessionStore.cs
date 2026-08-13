using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain contract to manage session thread metadata, lifecycle, and lineage.
/// Follows CS.01, CS.03, and Ambient Context isolation patterns.
/// </summary>
public interface IVKSessionStore
{
    /// <summary>
    /// Retrieves a session thread by its unique session ID within the current ambient identity context.
    /// </summary>
    Task<VKResult<VKSessionThread?>> GetSessionAsync(
        VKSessionId sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates a session thread in the store.
    /// </summary>
    Task<VKResult> UpdateSessionAsync(
        VKSessionThread session,
        CancellationToken cancellationToken = default);
}
