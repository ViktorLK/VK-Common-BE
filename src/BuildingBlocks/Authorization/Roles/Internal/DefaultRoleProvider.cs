using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Roles.Internal;

/// <summary>
/// A default implementation of <see cref="IVKRoleProvider"/> that uses dynamic RoleClaimType and role inheritance from options.
/// </summary>
internal sealed class DefaultRoleProvider(
    IOptions<VKRoleOptions> options,
    IOptions<VKAuthorizationOptions> globalOptions) : IVKRoleProvider
{
    private readonly VKRoleOptions _options = VKGuard.NotNull(options).Value;
    private readonly VKAuthorizationOptions _globalOptions = VKGuard.NotNull(globalOptions).Value;

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
        var inheritance = _options.RoleInheritance ?? [];

        var expandedRoles = ExpandUserRoles(user, claimType, inheritance);
        var hasRole = expandedRoles.Contains(role);

        return ValueTask.FromResult(VKResult.Success(hasRole));
    }

    private static HashSet<string> ExpandUserRoles(
        ClaimsPrincipal user,
        string claimType,
        Dictionary<string, string[]> inheritance)
    {
        var directRoles = user.FindAll(claimType).Select(c => c.Value)
            .Concat(user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var expanded = new HashSet<string>(directRoles, StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>(directRoles);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (inheritance.TryGetValue(current, out var children) && children is not null)
            {
                foreach (var child in children)
                {
                    if (expanded.Add(child))
                    {
                        queue.Enqueue(child);
                    }
                }
            }
        }

        return expanded;
    }
}
