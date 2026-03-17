using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VK.Blocks.ExceptionHandling.Abstractions;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;
using VK.Blocks.ExceptionHandling.Constants;
using VK.Blocks.ExceptionHandling.Helpers;
using VK.Blocks.Validation.Exceptions;

namespace VK.Blocks.ExceptionHandling.Handlers;

/// <summary>
/// Exception handler for <see cref="ValidationException"/>.
/// </summary>
public sealed class ValidationExceptionHandler(IProblemDetailsFactory factory, ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public bool CanHandle(ExceptionContext context)
        => context.Exception is ValidationException;

    public async Task HandleAsync(ExceptionContext context, CancellationToken ct)
    {
        var validationException = (ValidationException)context.Exception;

        logger.LogWarning(context.Exception, "Validation failed. TraceId: {TraceId}", context.TraceId);

        var problemDetails = factory.Create(
            context.HttpContext,
            validationException,
            StatusCodes.Status422UnprocessableEntity,
            ExceptionHandlingConstants.ErrorCodes.ValidationErrors);

        problemDetails.Extensions[ExceptionHandlingConstants.ExtensionKeys.Errors] = validationException.Errors;

        await ProblemDetailsHelper.WriteAsync(context.HttpContext, problemDetails, context.Exception, ct);
    }
}
