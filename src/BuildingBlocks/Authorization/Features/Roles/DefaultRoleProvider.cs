using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.DependencyInjection;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// A default implementation of <see cref="IRoleProvider"/> that uses dynamic RoleClaimType from options.
/// </summary>
public sealed class DefaultRoleProvider(IOptions<VKAuthorizationOptions> options) : IRoleProvider
{
    private readonly VKAuthorizationOptions _options = options.Value;

    /// <inheritdoc />
    public ValueTask<Result<bool>> IsInRoleAsync(ClaimsPrincipal user, string role, CancellationToken ct = default)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return ValueTask.FromResult(Result.Success(false));
        }

        // We check for the specific role claim type from options
        // Also supports user.IsInRole as a fallback or if it's the standard claim
        var hasRole = user.HasClaim(_options.RoleClaimType, role) || user.IsInRole(role);
        
        return ValueTask.FromResult(Result.Success(hasRole));
    }
}
