using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace VK.Blocks.APIStandards.Infrastructure;

/// <summary>
/// Global exception handler to standardize error responses.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
/// </remarks>
/// <param name="logger">The logger.</param>
/// <param name="env">The host environment.</param>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{
    #region Public Methods

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var validationProblemDetails = new HttpValidationProblemDetails(
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    ))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Detail = "One or more validation errors occurred."
            };

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await httpContext.Response
                .WriteAsJsonAsync(validationProblemDetails, cancellationToken);

            return true;
        }

        logger.LogError(
            exception,
            "Exception occurred: {Message}",
            exception.Message);

        // Rationale: General fallback for unexpected exceptions, ensuring a standardized 500 response.
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1",
            Detail = "An unexpected error occurred."
        };

        if (env.IsDevelopment())
        {
            problemDetails.Extensions["exception"] = new
            {
                Message = exception.Message,
                StackTrace = exception.StackTrace
            };
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    #endregion
}
