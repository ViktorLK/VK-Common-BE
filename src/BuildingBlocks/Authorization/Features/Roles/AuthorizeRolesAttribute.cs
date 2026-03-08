using System;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.Roles;

/// <summary>
/// Marks a controller or action as requiring one or more specific roles.
/// Replaces the built-in [Authorize(Roles = "")] syntax.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}


