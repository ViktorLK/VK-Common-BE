using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Persistence port interface for <see cref="VKTenantUser"/> aggregate root.
/// Follows AP.01, CS.01, and CS.03.
/// </summary>
public interface IVKIdentityTenantUserRepository
{
    /// <summary>
    /// Finds relation for a specific user within a specific tenant.
    /// </summary>
    Task<VKResult<VKTenantUser>> FindAsync(VKTenantId tenantId, VKUserId userId, CancellationToken ct = default);

    /// <summary>
    /// Lists all active relations for a user across all tenants.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKTenantUser>>> ListByUserAsync(VKUserId userId, CancellationToken ct = default);

    /// <summary>
    /// Lists all members belonging to a specific tenant.
    /// </summary>
    Task<VKResult<IReadOnlyList<VKTenantUser>>> ListByTenantAsync(VKTenantId tenantId, CancellationToken ct = default);

    /// <summary>
    /// Counts total members belonging to a tenant (useful for subscription quota validation).
    /// </summary>
    Task<VKResult<int>> CountByTenantAsync(VKTenantId tenantId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a membership relation exists for the given tenant and user.
    /// </summary>
    Task<bool> ExistsAsync(VKTenantId tenantId, VKUserId userId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new tenant user relation.
    /// </summary>
    Task<VKResult> AddAsync(VKTenantUser tenantUser, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing tenant user relation.
    /// </summary>
    Task<VKResult> UpdateAsync(VKTenantUser tenantUser, CancellationToken ct = default);

    /// <summary>
    /// Removes a user's relation from a tenant.
    /// </summary>
    Task<VKResult> RemoveAsync(VKTenantId tenantId, VKUserId userId, CancellationToken ct = default);
}
