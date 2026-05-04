using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Permissions.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPermissionProvider"/> that reads permissions from user claims.
/// </summary>
internal sealed class DefaultPermissionProvider(IOptions<VKPermissionOptions> options) : IVKPermissionProvider
{
    private readonly VKPermissionOptions _options = VKGuard.NotNull(options).Value;

    /// <inheritdoc />
    public ValueTask<VKResult<bool>> HasPermissionAsync(
        ClaimsPrincipal user,
        string VKPermission,
        CancellationToken ct = default)
    {
        VKGuard.NotNull(user);
        VKGuard.NotNullOrWhiteSpace(VKPermission);
        if (user.Identity?.IsAuthenticated != true)
        {
            return ValueTask.FromResult(VKResult.Success(false));
        }

        // Check for the specific VKPermission claim type from options
        var hasPermission = user.HasClaim(_options.PermissionClaimType, VKPermission);

        return ValueTask.FromResult(VKResult.Success(hasPermission));
    }
}
