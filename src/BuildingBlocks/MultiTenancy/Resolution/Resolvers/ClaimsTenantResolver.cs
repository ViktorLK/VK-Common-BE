using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;
using VK.Blocks.MultiTenancy.Options;

namespace VK.Blocks.MultiTenancy.Resolution.Resolvers;

/// <summary>
/// Resolves the tenant identifier from the authenticated user's JWT claims.
/// The claim type is configurable via <see cref="TenantResolutionOptions.ClaimType"/>.
/// </summary>
public sealed class ClaimsTenantResolver(TenantResolutionOptions options) : ITenantResolver
{
    #region Fields

    private readonly string _claimType = options.ClaimType;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int Order => 200;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public Task<TenantResolutionResult> ResolveAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var user = context.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            return Task.FromResult(
                TenantResolutionResult.Fail("User is not authenticated; cannot resolve tenant from claims."));
        }

        var tenantClaim = user.FindFirstValue(_claimType);

        if (!string.IsNullOrWhiteSpace(tenantClaim))
        {
            return Task.FromResult(TenantResolutionResult.Success(tenantClaim));
        }

        return Task.FromResult(
            TenantResolutionResult.Fail($"Claim '{_claimType}' not found on authenticated user."));
    }

    #endregion
}
