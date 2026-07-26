using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Middleware contract for pipeline processing onion chain.
/// Inherits from non-generic <see cref="IVKPipelineComponent"/>.
/// Follows AP.01.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
public interface IVKMiddleware<in TContext> : IVKPipelineComponent where TContext : class
{
    /// <summary>
    /// Gets the middleware execution order. Lower numbers execute earlier in the onion chain (outer layers).
    /// </summary>
    int MiddlewareOrder => 0;

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="next">The delegate representing the next middleware or terminal action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A non-generic VKResult.</returns>
    Task<VKResult> InvokeAsync(TContext context, VKPipelineDelegate next, CancellationToken cancellationToken = default);
}
