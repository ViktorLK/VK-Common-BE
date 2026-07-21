using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization;

/// <summary>
/// Marks a controller or action as requiring tenant isolation.
/// Ensures the user belongs to the same tenant as the resource being accessed.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class VKAuthorizeTenantIsolationAttribute : AuthorizeAttribute, IAuthorizationRequirementData
{
    /// <inheritdoc />
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return new VKTenantIsolationRequirement();
    }
}
