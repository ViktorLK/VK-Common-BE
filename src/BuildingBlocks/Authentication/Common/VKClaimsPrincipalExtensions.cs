using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Extension methods for <see cref="ClaimsPrincipal"/> to extract VK system identifiers.
/// </summary>
public static class VKClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user identifier from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The user identifier if found; otherwise, null.</returns>
    public static string? GetUserId(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? principal?.FindFirst(VKClaimConstants.UserId)?.Value;
    }

    /// <summary>
    /// Gets the JWT ID (jti) from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The JTI if found; otherwise, null.</returns>
    public static string? GetJti(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirst(VKClaimConstants.Jti)?.Value;
    }

    /// <summary>
    /// Gets the JWT ID (jti) from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The JTI if found; otherwise, null.</returns>
    public static VKAuthenticatedUser? GetVKAuthenticatedUser(this ClaimsPrincipal principal)
    {
        var userId = principal.FindFirst(VKClaimConstants.UserId)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return new VKAuthenticatedUser
        {
            Id = userId,
            Username = principal.FindFirst(VKClaimConstants.PreferredUsername)?.Value ?? "Unknown",
            TenantId = principal.FindFirst(VKClaimConstants.TenantId)?.Value,
            Email = principal.FindFirst(ClaimTypes.Email)?.Value
        };
    }

    /// <summary>
    /// Gets the username from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The username if found; otherwise, null.</returns>
    public static string? GetUsername(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirst(ClaimTypes.Name)?.Value
               ?? principal?.FindFirst(VKClaimConstants.PreferredUsername)?.Value;
    }

    /// <summary>
    /// Gets the email address from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The email address if found; otherwise, null.</returns>
    public static string? GetEmail(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Gets the tenant identifier from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The tenant identifier if found; otherwise, null.</returns>
    public static string? GetTenantId(this ClaimsPrincipal? principal)
    {
        return principal?.FindFirst(VKClaimConstants.TenantId)?.Value
               ?? principal?.FindFirst(VKClaimConstants.AzureTenantId)?.Value;
    }

    /// <summary>
    /// Gets the display name from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The display name if found; otherwise, null.</returns>
    public static string? GetDisplayName(this ClaimsPrincipal? principal)
    {
        var displayName = principal?.FindFirst(ClaimTypes.GivenName)?.Value;
        if (principal?.FindFirst(ClaimTypes.Surname)?.Value is { } surname)
        {
            displayName = string.IsNullOrWhiteSpace(displayName) ? surname : $"{displayName} {surname}";
        }

        return displayName ?? principal?.FindFirst(VKClaimConstants.Name)?.Value;
    }

    /// <summary>
    /// Gets the roles from the claims principal.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>A read-only list of roles.</returns>
    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal? principal)
    {
        // Concatenate standard role claims and VK-specific role claims.
        return principal?.FindAll(ClaimTypes.Role)
            .Concat(principal?.FindAll(VKClaimConstants.Role) ?? [])
            .Select(c => c.Value)
            .Distinct()
            .ToArray() ?? [];
    }

    /// <summary>
    /// Maps a <see cref="ClaimsPrincipal"/> to an <see cref="VKAuthenticatedUser"/> record.
    /// </summary>
    /// <param name="principal">The claims principal to map.</param>
    /// <returns>A <see cref="VKResult{T}"/> containing the mapped user, or an error if mandatory claims are missing.</returns>
    public static VKResult<VKAuthenticatedUser> ToAuthenticatedUser(this ClaimsPrincipal? principal)
    {
        if (principal is null)
        {
            return VKResult.Failure<VKAuthenticatedUser>(VKAuthenticationErrors.InvalidClaims);
        }

        var id = principal.GetUserId();
        var username = principal.GetUsername();

        // Integrity check: Id and Username are mandatory for our system
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(username))
        {
            return VKResult.Failure<VKAuthenticatedUser>(VKAuthenticationErrors.InvalidClaims);
        }

        var claimsDict = new Dictionary<string, string>();
        foreach (var claim in principal.Claims)
        {
            // Note: If multiple claims of the same type exist, only the first one is captured in this dictionary.
            claimsDict.TryAdd(claim.Type, claim.Value);
        }

        var user = new VKAuthenticatedUser
        {
            Id = id,
            Username = username,
            Email = principal.GetEmail(),
            TenantId = principal.GetTenantId(),
            DisplayName = principal.GetDisplayName(),
            Roles = principal.GetRoles(),
            Claims = claimsDict
        };

        return VKResult.Success(user);
    }
}
