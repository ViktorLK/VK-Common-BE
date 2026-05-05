using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Marks a controller or action as requiring one or more specific permissions.
/// Note: This class is NOT sealed because it is used as a base class by generated attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class VKAuthorizePermissionAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    /// <summary>
    /// Gets the list of permissions required.
    /// </summary>
    public ImmutableArray<string> Permissions { get; }

    /// <summary>
    /// Gets the evaluation mode (All/Any).
    /// </summary>
    public VKPermissionEvaluationMode Mode { get; init; } = VKPermissionEvaluationMode.All;

    /// <summary>
    /// Initializes a new instance with a single VKPermission.
    /// </summary>
    /// <param name="VKPermission">The VKPermission name.</param>
    public VKAuthorizePermissionAttribute(string VKPermission)
    {
        Permissions = [VKGuard.NotNullOrWhiteSpace(VKPermission)];
    }

    /// <summary>
    /// Initializes a new instance with multiple permissions.
    /// </summary>
    /// <param name="permissions">The permissions required.</param>
    protected VKAuthorizePermissionAttribute(params string[] permissions)
    {
        Permissions = [.. VKGuard.NotEmpty(permissions)];
    }

    /// <summary>
    /// Returns the authorization requirements defined by this attribute.
    /// </summary>
    /// <returns>A collection of requirements.</returns>
    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new VKPermissionRequirement(Permissions, Mode);
    }
}
