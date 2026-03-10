using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.MultiTenancy.Abstractions.Contracts;

namespace VK.Blocks.MultiTenancy.Resolution;

/// <summary>
/// Executes registered <see cref="ITenantResolver"/> instances in priority order
/// and returns the first successful resolution result.
/// </summary>
public sealed class TenantResolutionPipeline(
    IEnumerable<ITenantResolver> resolvers,
    ILogger<TenantResolutionPipeline> logger)
{
    #region Fields

    private readonly IReadOnlyList<ITenantResolver> _resolvers =
        resolvers.OrderBy(r => r.Order).ToList();

    private readonly ILogger<TenantResolutionPipeline> _logger = logger;

    #endregion

    #region Public Methods

    /// <summary>
    /// Executes all registered resolvers in order and returns the first successful result.
    /// If no resolver succeeds, returns a failure result.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="TenantResolutionResult"/> from the first successful resolver, or a failure.</returns>
    public async Task<TenantResolutionResult> ResolveAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        foreach (var resolver in _resolvers)
        {
            var result = await resolver.ResolveAsync(context, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogDebug(
                    "Tenant resolved by {ResolverType} with TenantId {TenantId}",
                    resolver.GetType().Name,
                    result.TenantId);

                return result;
            }

            _logger.LogTrace(
                "Resolver {ResolverType} did not resolve tenant: {Error}",
                resolver.GetType().Name,
                result.Error);
        }

        _logger.LogDebug("No resolver was able to resolve the tenant for the current request");

        return TenantResolutionResult.Fail("No resolver was able to resolve the tenant.");
    }

    #endregion
}
