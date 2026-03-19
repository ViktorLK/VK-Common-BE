using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core.Exceptions;
using VK.Blocks.ExceptionHandling.Abstractions;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;
using VK.Blocks.ExceptionHandling.Constants;
using VK.Blocks.ExceptionHandling.Helpers;

namespace VK.Blocks.ExceptionHandling.Handlers;

/// <summary>
/// Exception handler for "Not Found" exceptions.
/// </summary>
public sealed class NotFoundExceptionHandler(IProblemDetailsFactory factory, ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    public bool CanHandle(ExceptionContext context)
    {
        return context.Exception switch
        {
            KeyNotFoundException => true,
            BaseException baseEx when baseEx.StatusCode == StatusCodes.Status404NotFound => true,
            _ => false
        };
    }

    public async Task HandleAsync(ExceptionContext context, CancellationToken ct)
    {
        logger.LogWarning(context.Exception, "A resource was not found. TraceId: {TraceId}", context.TraceId);

        var errorCode = (context.Exception as BaseException)?.Code ?? ExceptionHandlingConstants.ErrorCodes.NotFound;
        var problemDetails = factory.Create(
            context.HttpContext,
            context.Exception,
            StatusCodes.Status404NotFound,
            errorCode);

        await ProblemDetailsHelper.WriteAsync(context.HttpContext, problemDetails, context.Exception, ct);
    }
}
