using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VK.Blocks.ExceptionHandling.Abstractions;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;
using VK.Blocks.ExceptionHandling.Constants;
using VK.Blocks.ExceptionHandling.Options;

namespace VK.Blocks.ExceptionHandling.Factories;

/// <summary>
/// Factory for creating <see cref="VKProblemDetails"/> instances.
/// </summary>
public sealed class ProblemDetailsFactory(IOptions<ExceptionHandlingOptions> options) : IProblemDetailsFactory
{
    private readonly ExceptionHandlingOptions _options = options.Value;

    /// <inheritdoc/>
    public VKProblemDetails Create(HttpContext context, Exception exception, int statusCode, string? errorCode = null)
    {
        var problemDetails = new VKProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = _options.ExposeStackTrace ? exception.Message : "An unexpected error occurred. Please contact support if the problem persists.",
            Instance = context.Request.Path,
            ErrorCode = errorCode,
            TraceId = context.TraceIdentifier
        };

        if (_options.ExposeStackTrace && context.Response.HasStarted == false)
        {
            problemDetails.Extensions[ExceptionHandlingConstants.ExtensionKeys.StackTrace] = exception.StackTrace;
        }

        return problemDetails;
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => ExceptionHandlingConstants.ProblemDetailsTitles.BadRequest,
        StatusCodes.Status401Unauthorized => ExceptionHandlingConstants.ProblemDetailsTitles.Unauthorized,
        StatusCodes.Status403Forbidden => ExceptionHandlingConstants.ProblemDetailsTitles.Forbidden,
        StatusCodes.Status404NotFound => ExceptionHandlingConstants.ProblemDetailsTitles.NotFound,
        StatusCodes.Status422UnprocessableEntity => "Validation Error", // Keeping existing logic
        StatusCodes.Status500InternalServerError => ExceptionHandlingConstants.ProblemDetailsTitles.InternalServerError,
        _ => "An error occurred"
    };
}
