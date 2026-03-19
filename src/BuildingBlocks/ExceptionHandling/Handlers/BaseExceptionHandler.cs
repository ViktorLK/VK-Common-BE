using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core.Exceptions;
using VK.Blocks.ExceptionHandling.Abstractions;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;
using VK.Blocks.ExceptionHandling.Helpers;

namespace VK.Blocks.ExceptionHandling.Handlers;

/// <summary>
/// Exception handler for <see cref="BaseException"/>.
/// </summary>
public sealed class BaseExceptionHandler(IProblemDetailsFactory factory, ILogger<BaseExceptionHandler> logger) : IExceptionHandler
{
    public bool CanHandle(ExceptionContext context)
        => context.Exception is BaseException;

    public async Task HandleAsync(ExceptionContext context, CancellationToken ct)
    {
        var baseException = (BaseException)context.Exception;

        logger.LogWarning(context.Exception, "Handled business exception: {ErrorCode}. TraceId: {TraceId}", baseException.Code, context.TraceId);

        var problemDetails = factory.Create(
            context.HttpContext,
            baseException,
            baseException.StatusCode,
            baseException.Code);

        await ProblemDetailsHelper.WriteAsync(context.HttpContext, problemDetails, context.Exception, ct);
    }
}
