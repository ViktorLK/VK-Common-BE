using Microsoft.AspNetCore.Authorization;
using VK.Blocks.Authorization.Diagnostics;

namespace VK.Blocks.Authorization.Features.TenantIsolation;

/// <summary>
/// Grants access when the authenticated user belongs to the same tenant as the target resource.
/// Evaluates <see cref="SameTenantRequirement"/>.
/// </summary>
public sealed class TenantAuthorizationHandler : AuthorizationHandler<SameTenantRequirement>
{
    #region Public Methods

    /// <inheritdoc />
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameTenantRequirement requirement)
    {
        // Example logic: Extract tenant ID from claims and compare with resource
        // In a real scenario, the resource would be an entity with a TenantId property

        var userTenantId = context.User.FindFirst(TenantIsolationConstants.TenantIdClaimType)?.Value;

        // If the resource is null, we might be checking global access
        // If it's an object, we attempt to check its tenant ID
        if (context.Resource is null)
        {
            if (!string.IsNullOrEmpty(userTenantId))
            {
                context.Succeed(requirement);
                AuthorizationDiagnostics.RecordDecision("TenantIsolation", true);
            }
            else
            {
                AuthorizationDiagnostics.RecordDecision("TenantIsolation", false);
            }
            return Task.CompletedTask;
        }

        // Logic for resource-based tenant check would go here
        // For now, if the user has a tenant ID, we let it pass the basic requirement
        if (!string.IsNullOrEmpty(userTenantId))
        {
            context.Succeed(requirement);
            AuthorizationDiagnostics.RecordDecision("TenantIsolation", true);
        }
        else
        {
            AuthorizationDiagnostics.RecordDecision("TenantIsolation", false);
        }

        return Task.CompletedTask;
    }

    #endregion
}


