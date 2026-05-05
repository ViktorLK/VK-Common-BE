using System.Security.Claims;
using Microsoft.Extensions.Options;


namespace VK.Blocks.Authorization.TenantIsolation.Internal;

/// <summary>
/// Default implementation of <see cref="IVKUserTenantProvider"/> that retrieves tenant ID from claims.
/// </summary>
internal sealed class DefaultUserTenantProvider(IOptions<VKTenantIsolationOptions> options) : IVKUserTenantProvider
{
    private readonly VKTenantIsolationOptions _options = options.Value;

    /// <inheritdoc />
    public string? GetUserTenantId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(_options.TenantClaimType);
    }
}
