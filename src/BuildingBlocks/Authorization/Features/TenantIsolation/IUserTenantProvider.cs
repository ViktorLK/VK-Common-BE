using System.Security.Claims;

namespace VK.Blocks.Authorization.Features.TenantIsolation;

/// <summary>
/// Provides access to the tenant information of a <see cref="ClaimsPrincipal"/>.
/// </summary>
public interface IUserTenantProvider
{
    /// <summary>
    /// Gets the tenant identifier associated with the specified user.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>The tenant identifier, or null if not found.</returns>
    string? GetUserTenantId(ClaimsPrincipal user);

    /// <summary>
    /// Checks if a user has a valid tenant identifier.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <returns>True if the user has a tenant ID; otherwise, false.</returns>
    bool HasTenantId(ClaimsPrincipal user) => !string.IsNullOrEmpty(GetUserTenantId(user));
}
