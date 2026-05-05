using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Provides user permissions for authorization checks.
/// </summary>
public interface IVKPermissionProvider
{
    /// <summary>
    /// Checks if the specified user has a particular VKPermission.
    /// </summary>
    ValueTask<VKResult<bool>> HasPermissionAsync(ClaimsPrincipal user, string VKPermission, CancellationToken ct = default);
}
