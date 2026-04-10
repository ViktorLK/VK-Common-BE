using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.DependencyInjection;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Permissions.Internal;

/// <summary>
/// Default implementation of <see cref="IPermissionProvider"/> that reads permissions from user claims.
/// </summary>
public sealed class DefaultPermissionProvider(IOptions<VKAuthorizationOptions> options) : IPermissionProvider
{
    private readonly VKAuthorizationOptions _options = options.Value;

    /// <inheritdoc />
    public ValueTask<Result<bool>> HasPermissionAsync(
        ClaimsPrincipal user, 
        string permission, 
        CancellationToken ct = default)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return ValueTask.FromResult(Result.Success(false));
        }

        // Check for the specific permission claim type from options
        var hasPermission = user.HasClaim(_options.PermissionClaimType, permission);
        
        return ValueTask.FromResult(Result.Success(hasPermission));
    }
}
