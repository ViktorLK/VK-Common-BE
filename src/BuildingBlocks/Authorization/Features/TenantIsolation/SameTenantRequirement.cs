using Microsoft.AspNetCore.Authorization;

namespace VK.Blocks.Authorization.Features.TenantIsolation;

/// <summary>
/// Requires the authenticated user to belong to the same tenant as the target resource.
/// Use with <c>TenantAuthorizationHandler</c>.
/// </summary>
public sealed record SameTenantRequirement : Microsoft.AspNetCore.Authorization.IAuthorizationRequirement
{
}



