using System.Collections.Generic;

namespace VK.Blocks.Core;

/// <summary>
/// Full security and authentication context expanding identity coordinates with RBAC details.
/// Inherits from <see cref="IVKIdentityContext"/>.
/// Follows AP.01.
/// </summary>
public interface IVKSecurityContext : IVKIdentityContext
{
    /// <summary>
    /// Gets the display name of the current user, or null if unauthenticated.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the assigned roles for the current user context.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Gets a value indicating whether the current execution user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
