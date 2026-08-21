namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Default ambient identity context providing clean fallback to <see cref="VKTenantId.Default"/> and <see cref="VKUserId.System"/>.
/// Follows AP.01.
/// </summary>
internal sealed class DefaultIdentityContext : IVKIdentityContext
{
    /// <summary>
    /// Gets a static singleton instance of <see cref="DefaultIdentityContext"/>.
    /// </summary>
    public static DefaultIdentityContext Instance { get; } = new(VKTenantId.Default, VKUserId.System);

    /// <inheritdoc />
    public VKTenantId TenantId { get; init; }

    /// <inheritdoc />
    public VKUserId UserId { get; init; }

    public DefaultIdentityContext(VKTenantId tenantId, VKUserId userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }
}
