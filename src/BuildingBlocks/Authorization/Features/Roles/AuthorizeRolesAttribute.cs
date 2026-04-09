using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// Marks a controller or action as requiring one or more specific roles.
/// </summary>
/// <param name="roles">The roles required. The user must be in at least one of these roles.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizeRolesAttribute(params string[] roles) : AuthorizeAttribute, IAuthorizationRequirementData
{
    #region Properties

    public string[] RequiredRoles { get; } = roles;

    #endregion

    #region Methods

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new RoleRequirement(RequiredRoles);
    }

    #endregion
}


