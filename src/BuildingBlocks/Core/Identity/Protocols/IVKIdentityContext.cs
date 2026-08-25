namespace VK.Blocks.Core;

/// <summary>
/// Minimal execution context providing strongly-typed TenantId and UserId identity coordinates.
/// Level 2 in the VK.Blocks 3-tier Identity Stratification model (inherits from <see cref="IVKTenantContext"/>).
/// Designed for domain stores, repositories, and infrastructure components.
/// Follows AP.01.
/// </summary>
public interface IVKIdentityContext : IVKTenantContext
{
    /// <summary>
    /// Gets the current strongly-typed UserId. Defaults to <see cref="VKUserId.Anonymous"/> when unauthenticated.
    /// </summary>
    VKUserId UserId { get; }
}
