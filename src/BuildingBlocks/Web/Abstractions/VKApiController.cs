using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

    private IActionResult CreateProblemDetails(VKError[] errors)
    {
        if (errors.Length == 0)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }

        // Rationale: If all errors are validation type, return a 400 with model state dictionary for
        // consistency with ASP.NET Core's built-in validation problem response format.
        if (errors.All(e => e.Type == VKErrorType.Validation))
        {
            var modelStateDictionary = new ModelStateDictionary();

            foreach (var error in errors)
            {
                modelStateDictionary.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(modelStateDictionary);
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
