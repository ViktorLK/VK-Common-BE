using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using VK.Blocks.Core;

namespace VK.Blocks.Identity.Context.Internal;

/// <summary>
/// Default immutable snapshot implementation of <see cref="IVKUserContext"/> backed by <see cref="IVKUserCoordinate"/> and optional <see cref="ClaimsPrincipal"/>.
/// Follows AP.01, AP.03, CS.01.
/// </summary>
internal sealed class DefaultUserContext : IVKUserContext
{
    private readonly ClaimsPrincipal? _principal;
    private IReadOnlyList<string>? _cachedRoles;
    private IReadOnlyCollection<Claim>? _cachedClaims;

    public DefaultUserContext(
        VKUserId userId,
        ClaimsPrincipal? principal = null,
        string? displayName = null,
        string? email = null,
        IReadOnlyList<string>? roles = null)
    {
        UserId = VKGuard.NotDefault(userId);
        _principal = principal;
        DisplayName = displayName ?? _principal?.FindFirst(ClaimTypes.Name)?.Value ?? _principal?.FindFirst("name")?.Value;
        Email = email ?? _principal?.FindFirst(ClaimTypes.Email)?.Value ?? _principal?.FindFirst("email")?.Value;
        _cachedRoles = roles;
    }

    public DefaultUserContext(
        IVKUserCoordinate userCoordinate,
        ClaimsPrincipal? principal = null,
        string? displayName = null,
        string? email = null,
        IReadOnlyList<string>? roles = null)
        : this(VKGuard.NotNull(userCoordinate).UserId, principal, displayName, email, roles)
    {
    }

    /// <inheritdoc />
    public VKUserId UserId { get; }

    /// <inheritdoc />
    public string? DisplayName { get; }

    /// <inheritdoc />
    public string? Email { get; }

    /// <inheritdoc />
    public IReadOnlyList<string> Roles
    {
        get
        {
            if (_cachedRoles is not null)
            {
                return _cachedRoles;
            }

            if (_principal is null)
            {
                return [];
            }

            _cachedRoles = _principal.FindAll(VKClaimConstants.Role)
                .Select(c => c.Value)
                .Concat(_principal.FindAll(ClaimTypes.Role).Select(c => c.Value))
                .Distinct()
                .ToList()
                .AsReadOnly();

            return _cachedRoles;
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<Claim> Claims
    {
        get
        {
            if (_cachedClaims is not null)
            {
                return _cachedClaims;
            }

            if (_principal is null)
            {
                return [];
            }

            _cachedClaims = _principal.Claims.ToList().AsReadOnly();
            return _cachedClaims;
        }
    }

    /// <inheritdoc />
    public string? FindClaimValue(string claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            return null;
        }

        return _principal?.FindFirst(claimType)?.Value;
    }
}
