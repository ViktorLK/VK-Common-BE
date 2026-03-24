using System.Security.Claims;
using VK.Blocks.Authentication.Claims;
using VK.Blocks.Authorization.Abstractions;

namespace VK.Labs.TaskManagement.Layered.Services.Implementations;

/// <summary>
/// A simple, claims-based implementation of IPermissionProvider for the TaskManagement lab.
/// This reads permissions directly from the JWT claims, avoiding database lookups during authorization.
/// </summary>
public sealed class TaskManagementPermissionProvider : IPermissionProvider
{
    public Task<IEnumerable<string>> GetUserPermissionsAsync(ClaimsPrincipal user, CancellationToken ct = default)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        var permissions = user.FindAll(VKClaimTypes.Permissions)
            .Select(c => c.Value)
            .ToList();

        return Task.FromResult(permissions.AsEnumerable());
    }

    public Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission, CancellationToken ct = default)
    {
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(permission))
        {
            return Task.FromResult(false);
        }

        var hasPermission = user.HasClaim(c => 
            c.Type == VKClaimTypes.Permissions && 
            c.Value.Equals(permission, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(hasPermission);
    }
}
