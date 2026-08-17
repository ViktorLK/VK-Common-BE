using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;
using VK.Blocks.Web.ProblemDetails.Internal;

namespace VK.Blocks.Web;

/// <summary>
/// A base controller for standard Web APIs, providing unified VKResult&lt;T&gt; to HTTP format mapping
/// without relying on CQRS/MediatR ISender dependencies.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class VKApiController : ControllerBase
{
    /// <summary>
    /// Gets the current trace identifier (Activity Trace ID or HttpContext Trace Identifier).
    /// </summary>
    protected string CurrentTraceId => Activity.Current?.Id ?? HttpContext.TraceIdentifier;

    /// <summary>
    /// Handles the result of an operation.
    /// </summary>
    /// <param name="result">The result to handle.</param>
    /// <returns>An <see cref="IActionResult"/> based on the result.</returns>
    protected IActionResult HandleResult(VKResult result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return CreateProblemDetails(result.Errors);
    }

    /// <summary>
    /// Handles the result of an operation with a value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to handle.</param>
    /// <returns>An <see cref="IActionResult"/> based on the result.</returns>
    protected IActionResult HandleResult<T>(VKResult<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Value switch
            {
                IVKPagedResult pagedResult => Ok(VKPagedResponse.Success(pagedResult)),
                _ => Ok(VKApiResponse.Success(result.Value))
            };
        }

        return CreateProblemDetails(result.Errors);
    }

    private ObjectResult CreateProblemDetails(VKError[] errors)
    {
        if (errors.Length == 0)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }

        if (errors.All(e => e.Type == VKErrorType.Validation))
        {
            var timeProvider = HttpContext.RequestServices.GetRequiredService<TimeProvider>();
            var problemDetails = new VKWebProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = "One or more validation errors occurred.",
                Instance = HttpContext.Request.Path,
                TraceId = CurrentTraceId,
                ErrorCode = "Web.Validation.Error",
                Timestamp = timeProvider.GetUtcNow(),
                Errors = errors.Select(e => new VKWebErrorDetail
                {
                    Code = e.Code,
                    Detail = e.Description
                }).ToList()
            };

            return new ObjectResult(problemDetails)
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
        }

        return Problem(errors[0]);
    }

    private ObjectResult Problem(VKError error)
    {
        var statusCode = error.Type.ToStatusCode();

        var problemDetails = new VKWebProblemDetails
        {
            Title = error.Type.ToString(),
            Detail = error.Description,
            Status = statusCode,
            ErrorCode = error.Code,
            TraceId = CurrentTraceId
        };

        return StatusCode(statusCode, problemDetails);
    }
}
