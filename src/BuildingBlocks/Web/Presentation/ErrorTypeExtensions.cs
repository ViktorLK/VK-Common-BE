using Microsoft.AspNetCore.Http;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Web.Presentation;

/// <summary>
/// Extensions for <see cref="ErrorType"/>.
/// </summary>
public static class ErrorTypeExtensions
{
    #region Public Methods

    /// <summary>
    /// Converts an <see cref="ErrorType"/> to the corresponding HTTP status code.
    /// </summary>
    /// <param name="errorType">The error type.</param>
    /// <returns>The HTTP status code.</returns>
    public static int ToStatusCode(this ErrorType errorType) => errorType switch
    {
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };

    #endregion
}
