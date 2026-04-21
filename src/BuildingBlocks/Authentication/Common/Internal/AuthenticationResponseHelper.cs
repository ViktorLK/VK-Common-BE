using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VK.Blocks.Authentication.Common.Internal;

/// <summary>
/// Internal helper to provide standardized authentication error responses.
/// </summary>
internal static class AuthenticationResponseHelper
{
    /// <summary>
    /// Writes a standardized 401 Unauthorized ProblemDetails response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="detail">The detailed error message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task WriteUnauthorizedResponseAsync(HttpContext context, string detail)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = VKAuthenticationConstants.ProblemJsonContentType;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = VKAuthenticationConstants.UnauthorizedTitle,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions[VKAuthenticationConstants.TraceIdExtension] = context.TraceIdentifier;

        return context.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: VKAuthenticationConstants.ProblemJsonContentType);
    }
}
