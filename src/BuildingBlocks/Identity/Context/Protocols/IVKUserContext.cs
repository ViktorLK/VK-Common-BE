using System.Collections.Generic;
using System.Security.Claims;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Rich domain-level user context extending the base coordinate <see cref="IVKUserCoordinate"/>.
/// Provides typed email, display name, roles, and claim evaluation capabilities.
/// Follows AP.01, AP.03, CS.01.
/// </summary>
public interface IVKUserContext : IVKUserCoordinate
{
    /// <summary>
    /// Gets the display name of the current user, or null if unauthenticated.
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// Gets the email address of the current user, or null if unauthenticated.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the assigned identity roles/tags, or empty if unassigned.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Gets the underlying collection of claims associated with the current user principal.
    /// </summary>
    IReadOnlyCollection<Claim> Claims { get; }

    /// <summary>
    /// Finds the first claim value matching the specified claim type.
    /// </summary>
    string? FindClaimValue(string claimType);
}
