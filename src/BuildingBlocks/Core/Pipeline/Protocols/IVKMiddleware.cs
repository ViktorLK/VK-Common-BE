using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Core;

/// <summary>
/// Defines a middleware that can intercept and participate in an execution flow.
/// </summary>
/// <typeparam name="TContext">The context type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IVKMiddleware<in TContext, TResponse> where TContext : class
{
    /// <summary>
    /// Gets the order in which the middleware is executed.
    /// </summary>
    int MiddlewareOrder { get; }

    /// <summary>
    /// Invokes the middleware logic.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="next">The delegate to invoke the next middleware or terminal action.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response result.</returns>
    Task<VKResult<TResponse>> InvokeAsync(
        TContext context,
        VKPipelineDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
