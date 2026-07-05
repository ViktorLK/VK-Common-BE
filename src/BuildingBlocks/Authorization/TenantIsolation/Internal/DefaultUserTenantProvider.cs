using System.Security.Claims;
using Microsoft.Extensions.Options;


namespace VK.Blocks.Authorization.TenantIsolation.Internal;

/// <summary>
/// Default implementation of <see cref="IVKUserTenantProvider"/> that retrieves tenant ID from claims.
/// </summary>
internal sealed class DefaultUserTenantProvider(
    IOptions<VKTenantIsolationOptions> options,
    IOptions<VKAuthorizationDefaultsOptions> globalOptions) : IVKUserTenantProvider
{
    private readonly VKTenantIsolationOptions _options = options.Value;
    private readonly VKAuthorizationDefaultsOptions _globalOptions = globalOptions.Value;

    /// <inheritdoc />
    public string? GetUserTenantId(ClaimsPrincipal user)
    {
        var claimType = _options.TenantClaimType ?? _globalOptions.TenantClaimType;
        return user.FindFirstValue(claimType);
    }
}
