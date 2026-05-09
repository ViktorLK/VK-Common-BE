using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Defines a handler for a specific type of exception.
/// </summary>
public interface IVKExceptionHandler
{
    /// <summary>
    /// Determines whether this handler can process the given exception.
    /// </summary>
    /// <param name="context">The exception context.</param>
    /// <returns>True if the handler can process the exception; otherwise, false.</returns>
    bool CanHandle(VKExceptionContext context);

    /// <summary>
    /// Handles the exception.
    /// </summary>
    /// <param name="context">The exception context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the handling and the updated context.</returns>
    ValueTask<VKResult<VKExceptionContext>> HandleAsync(VKExceptionContext context, CancellationToken ct);
}
