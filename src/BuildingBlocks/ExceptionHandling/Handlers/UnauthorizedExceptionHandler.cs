using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core.Exceptions;
using VK.Blocks.ExceptionHandling.Abstractions;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;
using VK.Blocks.ExceptionHandling.Constants;
using VK.Blocks.ExceptionHandling.Helpers;

namespace VK.Blocks.ExceptionHandling.Handlers;

/// <summary>
/// Exception handler for "Unauthorized" exceptions.
/// </summary>
public sealed class UnauthorizedExceptionHandler(IProblemDetailsFactory factory, ILogger<UnauthorizedExceptionHandler> logger) : IExceptionHandler
{
    public bool CanHandle(ExceptionContext context)
    {
        return context.Exception switch
        {
            UnauthorizedAccessException => true,
            BaseException baseEx when baseEx.StatusCode == StatusCodes.Status401Unauthorized => true,
            _ => false
        };
    }

    public async Task HandleAsync(ExceptionContext context, CancellationToken ct)
    {
        logger.LogWarning(context.Exception, "Unauthorized access attempt. TraceId: {TraceId}", context.TraceId);

        var errorCode = (context.Exception as BaseException)?.Code ?? ExceptionHandlingConstants.ErrorCodes.Unauthorized;
        var problemDetails = factory.Create(
            context.HttpContext,
            context.Exception,
            StatusCodes.Status401Unauthorized,
            errorCode);

        await ProblemDetailsHelper.WriteAsync(context.HttpContext, problemDetails, context.Exception, ct);
    }
}
