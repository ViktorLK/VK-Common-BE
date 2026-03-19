using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VK.Blocks.Core.Results;
using VK.Blocks.MultiTenancy.Constants;
using VK.Blocks.MultiTenancy.Options;

namespace VK.Blocks.MultiTenancy.Resolution.Resolvers;

/// <summary>
/// Resolves the tenant identifier from the authenticated user's JWT claims.
/// The claim type is configurable via <see cref="TenantResolutionOptions.ClaimType"/>.
/// </summary>
public sealed class ClaimsTenantResolver(IOptions<TenantResolutionOptions> options) : ITenantResolver
{
    #region Fields

    private readonly string _claimType = options.Value.ClaimType;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int Order => 200;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public Task<Result<string>> ResolveAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        var user = context.User;

        if (user.Identity is not { IsAuthenticated: true })
        {
            return Task.FromResult(Result.Failure<string>(MultiTenancyErrors.TenantNotFound));
        }

        var tenantClaim = user.FindFirstValue(_claimType);

        if (!string.IsNullOrWhiteSpace(tenantClaim))
        {
            return Task.FromResult(Result.Success(tenantClaim));
        }

        return Task.FromResult(Result.Failure<string>(MultiTenancyErrors.TenantNotFound));
    }

    #endregion
}
