using System;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Marks a controller or action as requiring a specific permission.
/// The permission is resolved dynamically via <see cref="VK.Blocks.Authorization.Permissions.PermissionPolicyProvider"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizePermissionAttribute : AuthorizeAttribute
{
    #region Constructors

    public AuthorizePermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = $"{PermissionsConstants.PolicyPrefix}{permission}";
    }

    #endregion

    #region Properties

    public string Permission { get; }

    #endregion
}


