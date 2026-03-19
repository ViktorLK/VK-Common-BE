using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VK.Blocks.Core.Exceptions;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;

namespace VK.Blocks.ExceptionHandling.Helpers;

internal static class ProblemDetailsHelper
{
    public static async Task WriteAsync(
        HttpContext httpContext,
        VKProblemDetails problemDetails,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is BaseException baseException)
        {
            foreach (var extension in baseException.Extensions)
            {
                problemDetails.Extensions[extension.Key] = extension.Value;
            }
        }

        if (problemDetails.Status.HasValue)
        {
            httpContext.Response.StatusCode = problemDetails.Status.Value;
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
    }
}
