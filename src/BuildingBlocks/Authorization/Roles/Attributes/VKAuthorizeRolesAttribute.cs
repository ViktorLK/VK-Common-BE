using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Marks a controller or action as requiring one or more specific roles.
/// </summary>
/// <param name="roles">The roles required. The user must be in at least one of these roles.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class VKAuthorizeRolesAttribute(params string[] roles) : AuthorizeAttribute, IAuthorizationRequirementData
{
    /// <summary>
    /// Gets the collection of required roles.
    /// </summary>
    public string[] RequiredRoles { get; } = VKGuard.NotEmpty(roles);

    /// <summary>
    /// Returns the authorization requirements defined by this attribute.
    /// </summary>
    /// <returns>A collection of requirements.</returns>
    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new VKRoleRequirement(RequiredRoles);
    }
}
