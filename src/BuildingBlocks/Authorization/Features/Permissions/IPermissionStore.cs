using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Permissions;

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
}
