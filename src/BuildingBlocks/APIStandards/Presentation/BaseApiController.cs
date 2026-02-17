using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using VK.Blocks.APIStandards.Shared;

namespace VK.Blocks.APIStandards.Presentation;

/// <summary>
/// Base controller for API controllers, providing common functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController(ISender sender) : ControllerBase
{
    #region Properties

    /// <summary>
    /// Gets the MediatR sender.
    /// </summary>
    protected ISender Sender => sender;

    #endregion

    #region Protected Methods

    /// <summary>
    /// Handles the result of a command or query.
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
    /// Handles the result of a command or query with a value.
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
            return Problem();
        }

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        return Problem(errors[0]);
    }

    private ObjectResult Problem(Error error)
        => Problem(statusCode: error.Type.ToStatusCode(), title: error.Description);

    private ActionResult ValidationProblem(Error[] errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        foreach (var error in errors)
        {
            modelStateDictionary.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem(modelStateDictionary);
    }

    #endregion
}
