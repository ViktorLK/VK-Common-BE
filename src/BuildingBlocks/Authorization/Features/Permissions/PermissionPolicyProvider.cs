using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.Features.Permissions;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Dynamic policy provider that creates <see cref="PermissionRequirement"/>-based policies
/// for policy names starting with "Permission:" prefix.
/// </summary>
public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null)
        {
            return policy;
        }

        if (policyName.StartsWith(PermissionsConstants.PolicyPrefix, System.StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[PermissionsConstants.PolicyPrefix.Length..];
            return new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
        }

        return null;
    }
}


