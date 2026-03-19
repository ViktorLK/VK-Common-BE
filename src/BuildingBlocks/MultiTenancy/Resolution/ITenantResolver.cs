using Microsoft.AspNetCore.Http;
using VK.Blocks.Core.Results;

namespace VK.Blocks.MultiTenancy.Resolution;

/// <summary>
/// Defines a strategy for resolving the tenant identifier from an HTTP request.
/// Multiple resolvers can be registered and executed in priority order by
/// the <see cref="TenantResolutionPipeline"/>.
/// </summary>
public interface ITenantResolver
{
    /// <summary>
    /// Gets the execution order of this resolver.
    /// Lower values indicate higher priority (executed first).
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Attempts to resolve the tenant identifier from the given HTTP context.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success with a tenant ID,
    /// or failure with an error description.
    /// </returns>
    Task<Result<string>> ResolveAsync(HttpContext context, CancellationToken cancellationToken = default);
}
