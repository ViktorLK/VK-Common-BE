using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.Context.Internal;

/// <summary>
/// Scoped dynamic accessor of <see cref="IVKUserContext"/> backed by <see cref="VKAmbientExecutionContext"/>.
/// Follows AP.01, AP.03, AP.06.
/// </summary>
internal sealed class UserContextAccessor : IVKUserContext
{
    private static IVKUserContext? ActiveContext => VKAmbientExecutionContext.Current?.User as IVKUserContext;

    /// <inheritdoc />
    public VKUserId UserId => VKAmbientExecutionContext.Current?.UserId ?? VKUserId.Anonymous;

    /// <inheritdoc />
    public string? DisplayName => ActiveContext?.DisplayName;

    /// <inheritdoc />
    public string? Email => ActiveContext?.Email;

    /// <inheritdoc />
    public IReadOnlyList<string> Roles => ActiveContext?.Roles ?? [];

    /// <inheritdoc />
    public IReadOnlyCollection<Claim> Claims => ActiveContext?.Claims ?? [];

    /// <inheritdoc />
    public string? FindClaimValue(string claimType) => ActiveContext?.FindClaimValue(claimType);
}
