using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VK.Blocks.Core;

namespace VK.Blocks.Web.UserContext.Internal;

/// <summary>
/// Provides access to the current authenticated user context from HttpContext.
/// Complies with CS.04 (Performance) by caching roles within the request scope.
/// </summary>
internal sealed class HttpContextUserContext(IHttpContextAccessor httpContextAccessor) : IVKSecurityContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = VKGuard.NotNull(httpContextAccessor);
    private IReadOnlyList<string>? _cachedRoles;

    /// <inheritdoc />
    public VKTenantId TenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return VKTenantId.Default;
            }

            // 1. Try standard VK claim
            var tenantIdStr = httpContext.User?.FindFirst(VKClaimConstants.TenantId)?.Value;
            if (!string.IsNullOrWhiteSpace(tenantIdStr) && VKTenantId.TryParse(tenantIdStr, null, out var parsedTenantId))
            {
                return parsedTenantId;
            }

            // 2. Fallback to identified TenantId from middleware (stored in HttpContext.Items)
            if (httpContext.Items.TryGetValue(WebConstants.Items.TenantId, out var identifiedValue) &&
                identifiedValue is string idStr &&
                VKTenantId.TryParse(idStr, null, out var itemTenantId))
            {
                return itemTenantId;
            }

            return VKTenantId.Default;
        }
    }

    /// <inheritdoc />
    public VKUserId UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdStr = user?.FindFirst(VKClaimConstants.UserId)?.Value
                   ?? user?.FindFirst("sub")?.Value
                   ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            return VKUserId.FromNullable(userIdStr);
        }
    }

    /// <inheritdoc />
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;



    /// <inheritdoc />
    public IReadOnlyList<string> Roles
    {
        get
        {
            if (_cachedRoles is not null)
            {
                return _cachedRoles;
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null)
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
