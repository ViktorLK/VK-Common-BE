using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Roles.Internal;

/// <summary>
/// A default implementation of <see cref="IVKRoleProvider"/> that uses dynamic RoleClaimType from options.
/// </summary>
internal sealed class DefaultRoleProvider(
    IOptions<VKRoleOptions> options,
    IOptions<VKAuthorizationDefaultsOptions> globalOptions) : IVKRoleProvider
{
    private readonly VKRoleOptions _options = VKGuard.NotNull(options).Value;
    private readonly VKAuthorizationDefaultsOptions _globalOptions = VKGuard.NotNull(globalOptions).Value;

    /// <inheritdoc />
    public ValueTask<VKResult<bool>> IsInRoleAsync(ClaimsPrincipal user, string role, CancellationToken ct = default)
    {
        VKGuard.NotNull(user);
        VKGuard.NotNullOrWhiteSpace(role);
        if (user.Identity?.IsAuthenticated != true)
        {
            return ValueTask.FromResult(VKResult.Success(false));
        }

        var claimType = _options.RoleClaimType ?? _globalOptions.RoleClaimType;
        var hasRole = user.HasClaim(claimType, role) || user.IsInRole(role);

        return ValueTask.FromResult(VKResult.Success(hasRole));
    }
}
