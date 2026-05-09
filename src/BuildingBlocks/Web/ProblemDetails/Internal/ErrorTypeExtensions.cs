using Microsoft.AspNetCore.Http;
using VK.Blocks.Core;

namespace VK.Blocks.Web.ProblemDetails.Internal;

/// <summary>
/// Extensions for <see cref="VKErrorType"/>.
/// </summary>
public static class ErrorTypeExtensions
{
    /// <summary>
    /// Converts an <see cref="VKErrorType"/> to the corresponding HTTP status code.
    /// </summary>
    /// <param name="errorType">The error type.</param>
    /// <returns>The HTTP status code.</returns>
    public static int ToStatusCode(this VKErrorType errorType) => errorType switch
    {
        VKErrorType.None => StatusCodes.Status200OK,
        VKErrorType.Validation => StatusCodes.Status400BadRequest,
        VKErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        VKErrorType.Forbidden => StatusCodes.Status403Forbidden,
        VKErrorType.NotFound => StatusCodes.Status404NotFound,
        VKErrorType.Conflict => StatusCodes.Status409Conflict,
        VKErrorType.PreconditionFailed => StatusCodes.Status412PreconditionFailed,
        VKErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
        VKErrorType.Failure => StatusCodes.Status500InternalServerError,
        VKErrorType.ExternalError => StatusCodes.Status502BadGateway,
        VKErrorType.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
        VKErrorType.Timeout => StatusCodes.Status504GatewayTimeout,
        _ => StatusCodes.Status500InternalServerError
    };
}
