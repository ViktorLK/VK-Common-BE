using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;
using VK.Blocks.ExceptionHandling.Diagnostics.Internal;

namespace VK.Blocks.ExceptionHandling.Pipelines.Internal;

/// <summary>
/// A default implementation of <see cref="IExceptionHandler"/> that converts
/// any unhandled exception into a domain <see cref="VKError"/>.
/// </summary>
internal sealed class DefaultExceptionHandler : IVKExceptionHandler
{
    private readonly ILogger<DefaultExceptionHandler> _logger;
    private readonly VKExceptionHandlingOptions _options;

    public DefaultExceptionHandler(ILogger<DefaultExceptionHandler> logger, IOptions<VKExceptionHandlingOptions> options)
    {
        _logger = VKGuard.NotNull(logger);
        _options = VKGuard.NotNull(options).Value;
    }

    public bool CanHandle(VKExceptionContext context)
    {
        VKGuard.NotNull(context);
        // This is a catch-all handler.
        return context.ErrorResponse is null;
    }

    public ValueTask<VKResult<VKExceptionContext>> HandleAsync(VKExceptionContext context, CancellationToken ct)
    {
        VKGuard.NotNull(context);

        // Log the unhandled exception with context (OR.01).
        _logger.UnhandledException(context.Exception, context.Exception.Message, context.TraceId);

        var description = VKExceptionHandlingErrors.Unhandled.Description;
        if (_options.IncludeExceptionDetails)
        {
            description = $"{description} Details: {context.Exception}";
        }

        // Convert exception to a framework-agnostic error response using non-destructive mutation.
        var updatedContext = context with
        {
            ErrorResponse = new VKErrorResponse
            {
                Code = VKExceptionHandlingErrors.Unhandled.Code,
                Description = description,
                Type = VKExceptionHandlingErrors.Unhandled.Type,
                TraceId = context.TraceId
            },
            Handled = true
        };

        return ValueTask.FromResult(VKResult.Success(updatedContext));
    }
}

