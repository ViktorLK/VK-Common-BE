using System.Collections.Generic;

namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// Default ambient security context providing clean fallback identity and unauthenticated security state.
/// Follows AP.01.
/// </summary>
internal sealed class DefaultSecurityContext : IVKSecurityContext
{
    /// <summary>
    /// Gets a static singleton instance of <see cref="DefaultSecurityContext"/>.
    /// </summary>
    public static DefaultSecurityContext Instance { get; } = new(VKTenantId.Default, VKUserId.System, null, [], false);

    /// <inheritdoc />
    public VKTenantId TenantId { get; init; }

    /// <inheritdoc />
    public VKUserId UserId { get; init; }

    /// <inheritdoc />
    public string? UserName { get; init; }

    /// <inheritdoc />
    public IReadOnlyList<string> Roles { get; init; }

    /// <inheritdoc />
    public bool IsAuthenticated { get; init; }

    public DefaultSecurityContext(
        VKTenantId tenantId,
        VKUserId userId,
        string? userName,
        IReadOnlyList<string> roles,
        bool isAuthenticated)
    {
        TenantId = tenantId;
        UserId = userId;
        UserName = userName;
        Roles = roles ?? [];
        IsAuthenticated = isAuthenticated;
    }
}
