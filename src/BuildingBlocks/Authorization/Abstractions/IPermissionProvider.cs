using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authorization.Abstractions;

/// <summary>
/// Provides user permissions for authorization checks.
/// </summary>
public interface IPermissionProvider
{
    /// <summary>
    /// Retrieves all permissions assigned to the specified user.
    /// </summary>
    Task<IEnumerable<string>> GetUserPermissionsAsync(ClaimsPrincipal user, CancellationToken ct = default);

    /// <summary>
    /// Checks if the specified user has a particular permission.
    /// </summary>
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default);
}
