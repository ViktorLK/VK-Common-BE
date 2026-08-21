using System.Collections.Generic;

namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Dynamic ambient dispatcher implementing <see cref="IVKSecurityContext"/>.
/// Resolves the live, real-time security context from <see cref="IVKSecurityContextAccessor"/> on every property access,
/// completely eliminating stale snapshot and captive dependency risks.
/// Follows [AP.01] and [AP.03].
/// </summary>
internal sealed class AmbientSecurityContextDispatcher : IVKSecurityContext
{
    private readonly IVKSecurityContextAccessor _accessor;

    public AmbientSecurityContextDispatcher(IVKSecurityContextAccessor accessor)
    {
        _accessor = VKGuard.NotNull(accessor);
    }

    /// <inheritdoc />
    public VKTenantId TenantId => _accessor.Current.TenantId;

    /// <inheritdoc />
    public VKUserId UserId => _accessor.Current.UserId;

    /// <inheritdoc />
    public string? UserName => _accessor.Current.UserName;

    /// <inheritdoc />
    public IReadOnlyList<string> Roles => _accessor.Current.Roles;

    /// <inheritdoc />
    public bool IsAuthenticated => _accessor.Current.IsAuthenticated;
}
