using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Provides user permissions for authorization checks.
/// </summary>
public interface IPermissionProvider
{
    /// <summary>
    /// Checks if the specified user has a particular permission.
    /// </summary>
    ValueTask<Result<bool>> HasPermissionAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default);
}
