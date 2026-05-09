using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace VK.Blocks.Web.ProblemDetails.Internal;

/// <summary>
/// Default implementation of <see cref="IVKProblemDetailsFactory"/> that creates
/// simplified <see cref="VKWebProblemDetails"/> instances.
/// </summary>
internal sealed class DefaultProblemDetailsFactory(TimeProvider timeProvider) : IVKProblemDetailsFactory
{
    /// <inheritdoc />
    public VKWebProblemDetails Create(HttpContext context, Exception exception, int statusCode, string? errorCode = null)
    {
        return new VKWebProblemDetails
        {
            Status = statusCode,
            Title = GetTitleForStatusCode(statusCode),
            Detail = exception.Message,
            Instance = context.Request.Path,
            TraceId = Activity.Current?.Id ?? context.TraceIdentifier,
            ErrorCode = errorCode ?? ProblemDetailsConstants.DefaultErrorCode,
            Timestamp = timeProvider.GetUtcNow()
        };
    }

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Internal Server Error"
    };
}
