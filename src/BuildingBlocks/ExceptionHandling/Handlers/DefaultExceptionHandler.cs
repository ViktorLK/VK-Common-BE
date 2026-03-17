using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.ExceptionHandling.Abstractions;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;
using VK.Blocks.ExceptionHandling.Constants;
using VK.Blocks.ExceptionHandling.Helpers;

namespace VK.Blocks.ExceptionHandling.Handlers;

/// <summary>
/// Fallback exception handler for unhandled exceptions.
/// </summary>
public sealed class DefaultExceptionHandler(
    IProblemDetailsFactory factory,
    ILogger<DefaultExceptionHandler> logger) : IExceptionHandler
{
    public bool CanHandle(ExceptionContext context) => true;

    public async Task HandleAsync(ExceptionContext context, CancellationToken ct)
    {
        logger.LogError(context.Exception, "An unhandled exception occurred during the request. TraceId: {TraceId}", context.TraceId);

        var problemDetails = factory.Create(
            context.HttpContext,
            context.Exception,
            StatusCodes.Status500InternalServerError,
            ExceptionHandlingConstants.ErrorCodes.InternalServerError);

        await ProblemDetailsHelper.WriteAsync(context.HttpContext, problemDetails, context.Exception, ct);
    }
}
