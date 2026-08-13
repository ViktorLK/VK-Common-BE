namespace VK.Blocks.Core;

/// <summary>
/// Extension methods for <see cref="VKBaseException"/> to enable fluent configuration.
/// </summary>
public static class VKBaseExceptionExtensions
{
    /// <summary>
    /// Adds an extension property to the exception and returns the concrete exception type for fluent chaining.
    /// </summary>
    /// <typeparam name="TException">The concrete exception type.</typeparam>
    /// <param name="ex">The exception instance.</param>
    /// <param name="key">The extension key.</param>
    /// <param name="value">The extension value.</param>
    /// <returns>The same exception instance.</returns>
    public static TException WithExtension<TException>(this TException ex, string key, object? value)
        where TException : VKBaseException
    {
        ex.SetExtension(key, value);
        return ex;
    }

    /// <summary>
    /// Converts a <see cref="VKBaseException"/> to a structured <see cref="VKError"/>.
    /// </summary>
    /// <param name="ex">The exception instance.</param>
    /// <returns>A structured <see cref="VKError"/> representing the exception.</returns>
    public static VKError ToError(this VKBaseException ex)
    {
        VKGuard.NotNull(ex);
        VKErrorType errorType = ex.StatusCode switch
        {
            401 => VKErrorType.Unauthorized,
            403 => VKErrorType.Forbidden,
            404 => VKErrorType.NotFound,
            409 => VKErrorType.Conflict,
            422 => VKErrorType.Validation,
            429 => VKErrorType.TooManyRequests,
            503 => VKErrorType.ServiceUnavailable,
            504 => VKErrorType.Timeout,
            _ when ex.StatusCode >= 500 => VKErrorType.ExternalError,
            _ => VKErrorType.Failure
        };

        return new VKError(ex.Code, ex.Message, errorType);
    }

    /// <summary>
    /// Converts a <see cref="VKBaseException"/> to a failed <see cref="VKResult"/>.
    /// </summary>
    /// <param name="ex">The exception instance.</param>
    /// <returns>A failed <see cref="VKResult"/> containing the mapped error.</returns>
    public static VKResult ToResult(this VKBaseException ex)
    {
        return VKResult.Failure(ex.ToError());
    }

    /// <summary>
    /// Converts a <see cref="VKBaseException"/> to a failed <see cref="VKResult{T}"/>.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="ex">The exception instance.</param>
    /// <returns>A failed <see cref="VKResult{T}"/> containing the mapped error.</returns>
    public static VKResult<T> ToResult<T>(this VKBaseException ex)
    {
        return VKResult.Failure<T>(ex.ToError());
    }
}
