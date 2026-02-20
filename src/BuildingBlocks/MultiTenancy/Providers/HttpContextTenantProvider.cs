using Microsoft.AspNetCore.Http;
using VK.Blocks.MultiTenancy.Abstractions;
using VK.Blocks.MultiTenancy.Constants;

namespace VK.Blocks.MultiTenancy.Providers;

/// <summary>
/// ITenantProvider implementation that resolves the TenantId from the current HttpContext headers.
/// </summary>
public sealed class HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc />
    public string? GetCurrentTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        if (httpContext.Request.Headers.TryGetValue(MultiTenancyConstants.Headers.TenantId, out var values))
        {
            var tenantId = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return tenantId;
            }
        }

        return null;
    }
}
