using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.Permissions;

/// <summary>
/// Marks a controller or action as requiring one or more specific permissions.
/// Note: This class is NOT sealed because it is used as a base class by generated attributes.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AuthorizePermissionAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    /// <summary>
    /// Gets the list of permissions required.
    /// </summary>
    public ImmutableArray<string> Permissions { get; }

    /// <summary>
    /// Gets the evaluation mode (All/Any).
    /// </summary>
    public PermissionEvaluationMode Mode { get; init; } = PermissionEvaluationMode.All;

    /// <summary>
    /// Initializes a new instance with a single permission.
    /// </summary>
    public AuthorizePermissionAttribute(string permission)
    {
        Permissions = [permission];
    }

    /// <summary>
    /// Initializes a new instance with multiple permissions.
    /// </summary>
    protected AuthorizePermissionAttribute(params string[] permissions)
    {
        Permissions = [.. permissions];
    }

    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new PermissionRequirement(Permissions, Mode);
    }
}


