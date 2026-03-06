using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VK.Blocks.Authentication.Validation;

/// <summary>
/// Internal helper to provide standardized authentication error responses.
/// </summary>
internal static class AuthenticationResponseHelper
{
    /// <summary>
    /// Writes a standardized 401 Unauthorized ProblemDetails response.
    /// </summary>
    public static Task WriteUnauthorizedResponseAsync(HttpContext context, string detail)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
