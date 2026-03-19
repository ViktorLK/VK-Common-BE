using VK.Blocks.ExceptionHandling.Abstractions.Contracts;

namespace VK.Blocks.ExceptionHandling.Abstractions;

/// <summary>
/// Defines a handler for a specific type of exception.
/// </summary>
public interface IExceptionHandler
{
    /// <summary>
    /// Determines whether this handler can process the given exception.
    /// </summary>
    /// <param name="context">The exception context.</param>
    /// <returns>True if the handler can process the exception; otherwise, false.</returns>
    bool CanHandle(ExceptionContext context);

    /// <summary>
    /// Handles the exception.
    /// </summary>
    /// <param name="context">The exception context.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(ExceptionContext context, CancellationToken ct);
}
