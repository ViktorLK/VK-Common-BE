namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Dynamic ambient dispatcher implementing <see cref="IVKIdentityContext"/>.
/// Resolves the live, real-time identity from <see cref="IVKIdentityContextAccessor"/> on every property access,
/// completely eliminating stale snapshot and captive dependency risks.
/// Follows [AP.01] and [AP.03].
/// </summary>
internal sealed class AmbientIdentityContextDispatcher : IVKIdentityContext
{
    private readonly IVKIdentityContextAccessor _accessor;

    public AmbientIdentityContextDispatcher(IVKIdentityContextAccessor accessor)
    {
        _accessor = VKGuard.NotNull(accessor);
    }

    /// <inheritdoc />
    public VKTenantId TenantId => _accessor.Current.TenantId;

    /// <inheritdoc />
    public VKUserId UserId => _accessor.Current.UserId;
}
