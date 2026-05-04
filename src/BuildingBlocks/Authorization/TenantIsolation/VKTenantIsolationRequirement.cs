using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Requirement for enforcing tenant isolation.
/// </summary>
public sealed record VKTenantIsolationRequirement : IVKAuthorizationRequirement
{
    /// <inheritdoc />
    public VKError DefaultError => VKAuthorizationErrors.TenantMismatch;
}
