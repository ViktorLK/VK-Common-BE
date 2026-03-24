using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VK.Blocks.Core.Results;
using VK.Blocks.ExceptionHandling.Abstractions.Contracts;

namespace VK.Blocks.Web.Presentation;

/// <summary>
/// A base controller for standard Web APIs, providing unified Result<T> to HTTP format mapping
/// without relying on CQRS/MediatR ISender dependencies.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class VKApiController : ControllerBase
{
    #region Protected Methods

    /// <summary>
    /// Handles the result of an operation.
    /// </summary>
    /// <param name="result">The result to handle.</param>
    /// <returns>An <see cref="IActionResult"/> based on the result.</returns>
    protected IActionResult HandleResult(IResult result)
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
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return CreateProblemDetails(result.Errors);
    }

    #endregion

    #region Private Methods

    private IActionResult CreateProblemDetails(Error[] errors)
    {
        if (errors.Length == 0)
        {
            return Problem(statusCode: Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError);
        }

        // Handle multiple errors (e.g. from Validation payload)
        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            // By VK.Blocks standard, returning multiple validation errors would ideally
            // be mapped through ValidationProblemDetails, but we unify around VKProblemDetails.
            // Using the standard ValidationProblem here handles the ModelState dictionary.
            var modelStateDictionary = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();

            foreach (var error in errors)
            {
                modelStateDictionary.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem(modelStateDictionary);
        }

        return Problem(errors[0]);
    }

    private ObjectResult Problem(Error error)
    {
        var statusCode = error.Type.ToStatusCode();

        var problemDetails = new VKProblemDetails
        {
            Title = error.Type.ToString(),
            Detail = error.Description,
            Status = statusCode,
            ErrorCode = error.Code,
            TraceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        return StatusCode(statusCode, problemDetails);
    }

    #endregion
}
