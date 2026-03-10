using Microsoft.AspNetCore.Http;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;
using VK.Blocks.MultiTenancy.Options;

namespace VK.Blocks.MultiTenancy.Resolution.Resolvers;

/// <summary>
/// Resolves the tenant identifier from the HTTP request header.
/// The header name is configurable via <see cref="TenantResolutionOptions.HeaderName"/>.
/// </summary>
public sealed class HeaderTenantResolver(TenantResolutionOptions options) : ITenantResolver
{
    #region Fields

    private readonly string _headerName = options.HeaderName;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int Order => 100;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public Task<TenantResolutionResult> ResolveAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Request.Headers.TryGetValue(_headerName, out var values))
        {
            var tenantId = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return Task.FromResult(TenantResolutionResult.Success(tenantId));
            }
        }

        return Task.FromResult(
            TenantResolutionResult.Fail($"Header '{_headerName}' not found or empty."));
    }

    #endregion
}
