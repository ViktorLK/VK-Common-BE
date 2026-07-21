using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.Permissions.Internal;

/// <summary>
/// Default implementation of <see cref="IVKPermissionProvider"/> that reads permissions from user claims and applies permission inheritance.
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

        var inheritance = _options.PermissionInheritance ?? [];
        var expandedPermissions = ExpandUserPermissions(user, _options.PermissionClaimType, inheritance);
        var hasPermission = expandedPermissions.Contains(VKPermission);

        return ValueTask.FromResult(VKResult.Success(hasPermission));
    }

    private static HashSet<string> ExpandUserPermissions(
        ClaimsPrincipal user,
        string claimType,
        Dictionary<string, string[]> inheritance)
    {
        var direct = user.FindAll(claimType).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var expanded = new HashSet<string>(direct, StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>(direct);

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
