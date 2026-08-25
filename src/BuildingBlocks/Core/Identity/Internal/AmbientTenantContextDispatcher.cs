namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Dynamic ambient dispatcher implementing <see cref="IVKTenantContext"/>.
/// Resolves the live, real-time tenant from <see cref="IVKTenantContextAccessor"/> on every property access,
/// completely eliminating stale snapshot and captive dependency risks.
/// Follows [AP.01] and [AP.03].
/// </summary>
internal sealed class AmbientTenantContextDispatcher(IVKTenantContextAccessor accessor) : IVKTenantContext
{
    private readonly IVKTenantContextAccessor _accessor = VKGuard.NotNull(accessor);

    /// <inheritdoc />
    public VKTenantId TenantId => _accessor.Current.TenantId;
}
