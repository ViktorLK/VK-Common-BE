using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VK.Blocks.Core;
using VK.Blocks.Web.Internal;

namespace VK.Blocks.Web.UserContext.Internal;

/// <summary>
/// Provides access to the current authenticated user context from HttpContext.
/// Complies with CS.04 (Performance) by caching roles within the request scope.
/// </summary>
internal sealed class HttpContextUserContext(IHttpContextAccessor httpContextAccessor) : IVKUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = VKGuard.NotNull(httpContextAccessor);
    private IReadOnlyList<string>? _cachedRoles;

    /// <inheritdoc />
    public string? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(VKClaimConstants.UserId)?.Value
                   ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user?.FindFirst("sub")?.Value;
        }
    }

    /// <inheritdoc />
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    /// <inheritdoc />
    public string? TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            // 1. Try standard VK claim
            var tenantId = httpContext.User?.FindFirst(VKClaimConstants.TenantId)?.Value;
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return tenantId;
            }

            // 2. Fallback to identified TenantId from middleware (stored in HttpContext.Items)
            if (httpContext.Items.TryGetValue(WebConstants.Items.TenantId, out var identifiedValue))
            {
                return identifiedValue?.ToString();
            }

            return null;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Roles
    {
        get
        {
            if (_cachedRoles != null)
            {
                return _cachedRoles;
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return [];
            }

            // Pattern: Optimized selection of roles from claims (CS.04)
            _cachedRoles = user.FindAll(VKClaimConstants.Role)
                .Select(c => c.Value)
                .Concat(user.FindAll(ClaimTypes.Role).Select(c => c.Value))
                .Distinct()
                .ToList()
                .AsReadOnly();

            return _cachedRoles;
        }
    }

    /// <inheritdoc />
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}

