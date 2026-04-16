using System.Collections.Generic;

namespace VK.Blocks.Core.Abstractions.Internal;

/// <summary>
/// A neutral, unauthenticated implementation of <see cref="IUserContext"/>.
/// Useful for testing, background tasks, or as a default fallback.
/// </summary>
internal sealed class NullUserContext : IUserContext
{
    private static readonly IReadOnlyList<string> EmptyRoles = [];

    /// <inheritdoc />
    public string? UserId => null;

    /// <inheritdoc />
    public string? UserName => null;

    /// <inheritdoc />
    public string? TenantId => null;

    /// <inheritdoc />
    public IReadOnlyList<string> Roles => EmptyRoles;

    /// <inheritdoc />
    public bool IsAuthenticated => false;
}
