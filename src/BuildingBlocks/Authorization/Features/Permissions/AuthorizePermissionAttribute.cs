using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Marks a controller or action as requiring a specific permission.
/// Note: This class is NOT sealed because it is used as a base class by PermissionsCatalogGenerator.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AuthorizePermissionAttribute(string permission) : AuthorizeAttribute, IAuthorizationRequirementData
{

    #region Properties

    public string Permission { get; } = permission;

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new PermissionRequirement(Permission);
    }

    #endregion
}


