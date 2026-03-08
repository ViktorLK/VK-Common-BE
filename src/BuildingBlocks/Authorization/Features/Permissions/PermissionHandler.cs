using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authorization.Abstractions;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Evaluates <see cref="PermissionRequirement"/> by delegating to <see cref="IPermissionProvider"/>.
/// Also implements <see cref="IAuthorizationHandler"/> for direct programmatic authorization checks.
/// </summary>
public sealed class PermissionHandler(IPermissionProvider permissionProvider)
    : AuthorizationHandler<PermissionRequirement>, VK.Blocks.Authorization.Abstractions.IVKAuthorizationHandler
{
    #region Public Methods

    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (await permissionProvider.HasPermissionAsync(context.User, requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }

    /// <inheritdoc />
    public async Task<bool> AuthorizeAsync(ClaimsPrincipal user, object? resource, string requirement, CancellationToken ct = default)
    {
        return await permissionProvider.HasPermissionAsync(user, requirement, ct);
    }

    #endregion
}



