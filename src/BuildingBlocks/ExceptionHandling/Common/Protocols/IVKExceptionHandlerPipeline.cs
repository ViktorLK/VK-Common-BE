using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Defines a pipeline for processing exceptions through multiple handlers.
/// </summary>
public interface IVKExceptionHandlerPipeline
{
    /// <summary>
    /// Processes the exception through the registered handlers.
    /// </summary>
    /// <param name="context">The exception context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the handling and the updated context.</returns>
    ValueTask<VKResult<VKExceptionContext>> HandleAsync(VKExceptionContext context, CancellationToken ct);
}
