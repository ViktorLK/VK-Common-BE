using System.Collections.Generic;

namespace VK.Blocks.Core.Identity.Internal;

/// <summary>
/// A neutral, unauthenticated implementation of <see cref="IVKUserContext"/>.
/// Useful for testing, background tasks, or as a default fallback.
/// </summary>
internal sealed class NullUserContext : IVKUserContext
{
    private static readonly IReadOnlyList<string> _emptyRoles = [];

    /// <inheritdoc />
    public string? UserId => null;

    /// <inheritdoc />
    public string? UserName => null;

    /// <inheritdoc />
    public string? TenantId => null;

    /// <inheritdoc />
    public IReadOnlyList<string> Roles => _emptyRoles;

    /// <inheritdoc />
    public bool IsAuthenticated => false;
}
