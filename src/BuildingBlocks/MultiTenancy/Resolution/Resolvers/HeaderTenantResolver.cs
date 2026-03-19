using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VK.Blocks.Core.Results;
using VK.Blocks.MultiTenancy.Constants;
using VK.Blocks.MultiTenancy.Options;

namespace VK.Blocks.MultiTenancy.Resolution.Resolvers;

/// <summary>
/// Resolves the tenant identifier from a configurable HTTP header.
/// The header name is configurable via <see cref="TenantResolutionOptions.HeaderName"/>.
/// </summary>
public sealed class HeaderTenantResolver(IOptions<TenantResolutionOptions> options) : ITenantResolver
{
    #region Fields

    private readonly string _headerName = options.Value.HeaderName;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int Order => 100;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public Task<Result<string>> ResolveAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Request.Headers.TryGetValue(_headerName, out var values))
        {
            var tenantId = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return Task.FromResult(Result.Success(tenantId));
            }
        }

        return Task.FromResult(Result.Failure<string>(MultiTenancyErrors.TenantNotFound));
    }

    #endregion
}
