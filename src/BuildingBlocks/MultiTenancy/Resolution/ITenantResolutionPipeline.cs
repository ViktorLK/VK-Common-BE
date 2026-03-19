using Microsoft.AspNetCore.Http;
using VK.Blocks.Core.Results;

namespace VK.Blocks.MultiTenancy.Resolution;

/// <summary>
/// Defines a pipeline for resolving the current tenant from an HTTP request.
/// </summary>
public interface ITenantResolutionPipeline
{
    /// <summary>
    /// Executes all registered resolvers in order and returns the first successful result.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Result{T}"/> from the first successful resolver, or a failure.</returns>
    Task<Result<string>> ResolveAsync(
        HttpContext context,
        CancellationToken cancellationToken = default);
}
