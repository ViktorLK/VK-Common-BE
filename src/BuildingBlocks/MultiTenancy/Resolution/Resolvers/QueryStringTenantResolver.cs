using Microsoft.AspNetCore.Http;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;
using VK.Blocks.MultiTenancy.Options;

namespace VK.Blocks.MultiTenancy.Resolution.Resolvers;

/// <summary>
/// Resolves the tenant identifier from the request query string.
/// Intended for use in development environments only.
/// The parameter name is configurable via <see cref="TenantResolutionOptions.QueryStringParameterName"/>.
/// </summary>
public sealed class QueryStringTenantResolver(TenantResolutionOptions options) : ITenantResolver
{
    #region Fields

    private readonly string _parameterName = options.QueryStringParameterName;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int Order => 900;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public Task<TenantResolutionResult> ResolveAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Request.Query.TryGetValue(_parameterName, out var values))
        {
            var tenantId = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return Task.FromResult(TenantResolutionResult.Success(tenantId));
            }
        }

        return Task.FromResult(
            TenantResolutionResult.Fail($"Query string parameter '{_parameterName}' not found or empty."));
    }

    #endregion
}
