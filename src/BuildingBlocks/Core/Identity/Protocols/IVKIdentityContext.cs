namespace VK.Blocks.Core;

/// <summary>
/// Minimal execution context providing strongly-typed TenantId and UserId identity coordinates.
/// Designed for domain stores, repositories, and infrastructure components.
/// Follows AP.01.
/// </summary>
public interface IVKIdentityContext
{
    /// <summary>
    /// Gets the current strongly-typed TenantId. Defaults to <see cref="VKTenantId.Default"/> when in single-tenant/unauthenticated context.
    /// </summary>
    VKTenantId TenantId { get; }

    /// <summary>
    /// Gets the current strongly-typed UserId. Defaults to <see cref="VKUserId.Anonymous"/> when unauthenticated.
    /// </summary>
    VKUserId UserId { get; }
}
