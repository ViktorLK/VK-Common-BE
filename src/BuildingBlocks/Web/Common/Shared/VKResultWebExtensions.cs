using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VK.Blocks.Core;
using VK.Blocks.Web.ProblemDetails.Internal;

namespace VK.Blocks.Web;

/// <summary>
/// Provides extension methods for converting <see cref="VKResult"/> and <see cref="VKResult{T}"/>
/// into standard ASP.NET Core <see cref="IActionResult"/> responses adhering to RFC 7807 and VK.Blocks conventions.
/// Follows AP.01, CS.01, and CS.02.
/// </summary>
public static class VKResultWebExtensions
{
    /// <summary>
    /// Converts a <see cref="VKResult"/> into an <see cref="IActionResult"/>.
    /// Returns 204 NoContent on success, or an RFC 7807 ProblemDetails on failure.
    /// </summary>
    /// <param name="result">The result instance.</param>
    /// <returns>An <see cref="IActionResult"/> representing the HTTP response.</returns>
    public static IActionResult ToActionResult(this VKResult result)
    {
        VKGuard.NotNull(result);

        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return ToProblemDetailsResult(result.Errors);
    }

    /// <summary>
    /// Converts a <see cref="VKResult{T}"/> into an <see cref="IActionResult"/>.
    /// Returns 200 OK with the value on success, or an RFC 7807 ProblemDetails on failure.
    /// </summary>
    /// <typeparam name="T">The result payload type.</typeparam>
    /// <param name="result">The result instance.</param>
    /// <returns>An <see cref="IActionResult"/> representing the HTTP response.</returns>
    public static IActionResult ToActionResult<T>(this VKResult<T> result)
    {
        VKGuard.NotNull(result);

        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return ToProblemDetailsResult(result.Errors);
    }

    /// <summary>
    /// Converts a <see cref="VKResult{T}"/> into a 201 CreatedAtAction <see cref="IActionResult"/> on success,
    /// or an RFC 7807 ProblemDetails on failure.
    /// </summary>
    /// <typeparam name="T">The result payload type.</typeparam>
    /// <param name="result">The result instance.</param>
    /// <param name="actionName">The name of the action to use for generating the URL.</param>
    /// <param name="routeValues">The route data to use for generating the URL.</param>
    /// <returns>An <see cref="IActionResult"/> representing the HTTP response.</returns>
    public static IActionResult ToCreatedAtActionResult<T>(
        this VKResult<T> result,
        string? actionName,
        object? routeValues)
    {
        VKGuard.NotNull(result);

        if (result.IsSuccess)
        {
            return new CreatedAtActionResult(actionName, null, routeValues, result.Value);
        }

        return ToProblemDetailsResult(result.Errors);
    }

    /// <summary>
    /// Converts a <see cref="VKResult{T}"/> into a 201 CreatedAtAction <see cref="IActionResult"/> on success,
    /// generating route values dynamically from the successful value.
    /// </summary>
    /// <typeparam name="T">The result payload type.</typeparam>
    /// <param name="result">The result instance.</param>
    /// <param name="actionName">The name of the action to use for generating the URL.</param>
    /// <param name="routeValuesFactory">The function to produce route data from the success payload.</param>
    /// <returns>An <see cref="IActionResult"/> representing the HTTP response.</returns>
    public static IActionResult ToCreatedAtActionResult<T>(
        this VKResult<T> result,
        string? actionName,
        System.Func<T, object?> routeValuesFactory)
    {
        VKGuard.NotNull(result);
        VKGuard.NotNull(routeValuesFactory);

        if (result.IsSuccess)
        {
            var routeValues = routeValuesFactory(result.Value);
            return new CreatedAtActionResult(actionName, null, routeValues, result.Value);
        }

        return ToProblemDetailsResult(result.Errors);
    }

    /// <summary>
    /// Converts a <see cref="VKError"/> into an RFC 7807 <see cref="VKWebProblemDetails"/> ObjectResult.
    /// </summary>
    /// <param name="error">The VK error.</param>
    /// <returns>An <see cref="ObjectResult"/> containing <see cref="VKWebProblemDetails"/>.</returns>
    public static IActionResult ToProblemDetailsResult(this VKError error)
    {
        VKGuard.NotNull(error);
        return ToProblemDetailsResult([error]);
    }

    /// <summary>
    /// Converts an array of <see cref="VKError"/> into an RFC 7807 <see cref="VKWebProblemDetails"/> ObjectResult.
    /// </summary>
    /// <param name="errors">The array of errors.</param>
    /// <returns>An <see cref="ObjectResult"/> containing <see cref="VKWebProblemDetails"/>.</returns>
    public static IActionResult ToProblemDetailsResult(this VKError[] errors)
    {
        VKGuard.NotNull(errors);

        var traceId = Activity.Current?.Id;

        if (errors.Length == 0)
        {
            var internalErrorDetails = new VKWebProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                ErrorCode = "System.InternalError",
                TraceId = traceId
            };
            return new ObjectResult(internalErrorDetails) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        if (errors.All(e => e.Type == VKErrorType.Validation))
        {
            var validationDetails = new VKWebProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "One or more validation errors occurred.",
                ErrorCode = "Web.Validation.Error",
                TraceId = traceId,
                Errors = errors.Select(e => new VKWebErrorDetail
                {
                    Code = e.Code,
                    Detail = e.Description
                }).ToList()
            };

            return new ObjectResult(validationDetails) { StatusCode = StatusCodes.Status400BadRequest };
        }

        var primary = errors[0];
        var statusCode = primary.Type.ToStatusCode();

        var problemDetails = new VKWebProblemDetails
        {
            Title = primary.Type.ToString(),
            Detail = primary.Description,
            Status = statusCode,
            ErrorCode = primary.Code,
            TraceId = traceId
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
