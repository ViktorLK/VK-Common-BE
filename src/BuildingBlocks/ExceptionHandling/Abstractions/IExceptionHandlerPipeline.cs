using VK.Blocks.ExceptionHandling.Abstractions.Contracts;

namespace VK.Blocks.ExceptionHandling.Abstractions;

/// <summary>
/// Defines a pipeline for processing exceptions through multiple handlers.
/// </summary>
public interface IExceptionHandlerPipeline
{
    /// <summary>
    /// Processes the exception through the registered handlers.
    /// </summary>
    /// <param name="context">The exception context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(ExceptionContext context, CancellationToken ct);
}
