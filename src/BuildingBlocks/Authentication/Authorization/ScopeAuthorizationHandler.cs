using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authentication.Claims;

namespace VK.Blocks.Authentication.Authorization;

/// <summary>
/// Evaluates whether the user satisfies the <see cref="ScopeRequirement"/>.
/// </summary>
public sealed class ScopeAuthorizationHandler : AuthorizationHandler<ScopeRequirement>
{
    /// <inheritdoc />
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
    {
        // Check if the user has a scope claim that matches the required scope
        if (context.User.HasClaim(c => c.Type == VKClaimTypes.Scope && c.Value == requirement.Scope))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
