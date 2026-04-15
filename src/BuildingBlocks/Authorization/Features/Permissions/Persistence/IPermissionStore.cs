using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Authorization.Features.Permissions.Metadata;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Permissions.Persistence;

/// <summary>
/// Defines a contract for synchronizing permission metadata with a persistent store.
/// </summary>
public interface IPermissionStore
{
    /// <summary>
    /// Synchronizes the provided permission definitions with the persistent store.
    /// This should handle creating new permissions, updating existing ones, or marking old ones as inactive.
    /// </summary>
    /// <param name="permissions">The collection of permissions to synchronize.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    ValueTask<Result> SyncPermissionsAsync(IEnumerable<Permission> permissions, CancellationToken ct = default);

    /// <summary>
    /// Checks if the specified user has a particular permission in the store.
    /// </summary>
    /// <param name="user">The user (claims principal) to evaluate.</param>
    /// <param name="permission">The permission name to check.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A result indicating whether the permission is granted.</returns>
    ValueTask<Result<bool>> HasPermissionAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default);
}
