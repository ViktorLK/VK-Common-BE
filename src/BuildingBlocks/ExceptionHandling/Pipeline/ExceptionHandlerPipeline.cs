using VK.Blocks.ExceptionHandling.Abstractions;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;

namespace VK.Blocks.ExceptionHandling.Pipeline;

/// <summary>
/// Default implementation of the exception handling pipeline.
/// </summary>
public sealed class ExceptionHandlerPipeline(IEnumerable<IExceptionHandler> handlers) : IExceptionHandlerPipeline
{
    public async Task HandleAsync(ExceptionContext context, CancellationToken ct)
    {
        foreach (var handler in handlers)
        {
            if (handler.CanHandle(context))
            {
                await handler.HandleAsync(context, ct);
                context.Handled = true;
                break;
            }
        }
    }
}
