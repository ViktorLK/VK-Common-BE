using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Defines a contract for synchronizing VKPermission metadata with a persistent store.
/// </summary>
public interface IVKPermissionStore
{
    /// <summary>
    /// Synchronizes the provided VKPermission definitions with the persistent store.
    /// This should handle creating new permissions, updating existing ones, or marking old ones as inactive.
    /// </summary>
    /// <param name="permissions">The collection of permissions to synchronize.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    ValueTask<VKResult> SyncPermissionsAsync(IEnumerable<VKPermission> permissions, CancellationToken ct = default);

    /// <summary>
    /// Checks if the specified user has a particular VKPermission in the store.
    /// </summary>
    /// <param name="user">The user (claims principal) to evaluate.</param>
    /// <param name="VKPermission">The VKPermission name to check.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A result indicating whether the VKPermission is granted.</returns>
    ValueTask<VKResult<bool>> HasPermissionAsync(ClaimsPrincipal user, string VKPermission, CancellationToken ct = default);
}
