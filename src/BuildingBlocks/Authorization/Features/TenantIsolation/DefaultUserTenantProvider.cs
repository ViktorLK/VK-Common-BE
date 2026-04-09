using System.Security.Claims;
using Microsoft.Extensions.Options;
using VK.Blocks.Authorization.DependencyInjection;

namespace VK.Blocks.Authorization.Features.TenantIsolation;

/// <inheritdoc />
public sealed class DefaultUserTenantProvider(IOptions<VKAuthorizationOptions> options) : IUserTenantProvider
{
    private readonly VKAuthorizationOptions _options = options.Value;

    /// <inheritdoc />
    public string? GetUserTenantId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(_options.TenantClaimType);
    }
}
