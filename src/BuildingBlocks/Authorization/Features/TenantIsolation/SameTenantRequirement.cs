using VK.Blocks.Authorization.Abstractions;
using VK.Blocks.Authorization.Common;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authorization.Features.TenantIsolation;

/// <summary>
/// Requires the authenticated user to belong to the same tenant as the target resource.
/// Use with <c>TenantAuthorizationHandler</c>.
/// </summary>
public sealed record SameTenantRequirement : IVKAuthorizationRequirement
{
    /// <summary>
    /// Gets the default error associated with the requirement failure.
    /// </summary>
    /// <inheritdoc />
    public Error DefaultError => AuthorizationErrors.TenantMismatch;
}
