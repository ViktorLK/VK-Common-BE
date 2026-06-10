using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Middleware interface for controlling the flow of Psyche pipeline executions (Onion model).
/// Follows CS.01, CS.03.
/// </summary>
public interface IVKPsycheMiddleware
{
    /// <summary>
    /// Gets the execution order of the middleware. Lower values run first.
    /// </summary>
    int MiddlewareOrder { get; }

    /// <summary>
    /// Invokes the middleware logic.
    /// </summary>
    Task<VKResult<VKPsycheResponse>> InvokeAsync(
        VKPsycheContext context,
        VKPsycheMiddlewareDelegate next,
        CancellationToken cancellationToken);
}
