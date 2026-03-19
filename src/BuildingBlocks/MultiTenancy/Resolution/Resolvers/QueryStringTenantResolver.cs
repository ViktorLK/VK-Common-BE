using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VK.Blocks.Core.Results;
using VK.Blocks.MultiTenancy.Constants;
using VK.Blocks.MultiTenancy.Options;

namespace VK.Blocks.MultiTenancy.Resolution.Resolvers;

/// <summary>
/// Resolves the tenant identifier from the request query string.
/// Intended for use in development environments only.
/// The parameter name is configurable via <see cref="TenantResolutionOptions.QueryStringParameterName"/>.
/// </summary>
public sealed class QueryStringTenantResolver(
    IOptions<TenantResolutionOptions> options,
    IHostEnvironment environment) : ITenantResolver
{
    #region Fields

    private readonly string _parameterName = options.Value.QueryStringParameterName;
    private readonly IHostEnvironment _environment = environment;

    #endregion

    #region Properties

    /// <inheritdoc />
    public int Order => 900;

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public Task<Result<string>> ResolveAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment())
        {
            return Task.FromResult(Result.Failure<string>(MultiTenancyErrors.ResolverNotAllowed));
        }

        if (context.Request.Query.TryGetValue(_parameterName, out var values))
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
