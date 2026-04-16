namespace VK.Blocks.Core.Results;

/// <summary>
/// Represents a standardized error with a code and description.
/// </summary>
/// <param name="Code">The unique error code.</param>
/// <param name="Description">The error description.</param>
/// <param name="Type">The error type (e.g., Validation, NotFound, Failure).</param>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    /// <summary>
    /// Represents no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    /// <summary>
    /// Represents a null value error.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.", ErrorType.Failure);

    /// <summary>
    /// Represents a condition not met error.
    /// </summary>
    public static readonly Error ConditionNotMet = new("Error.ConditionNotMet", "The specified condition was not met.", ErrorType.Failure);

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A validation error.</returns>
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>An unauthorized error.</returns>
    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A forbidden error.</returns>
    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A not found error.</returns>
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A conflict error.</returns>
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

    /// <summary>
    /// Creates a precondition failed error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A precondition failed error.</returns>
    public static Error PreconditionFailed(string code, string description) => new(code, description, ErrorType.PreconditionFailed);

    /// <summary>
    /// Creates a too many requests error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A too many requests error.</returns>
    public static Error TooManyRequests(string code, string description) => new(code, description, ErrorType.TooManyRequests);

    /// <summary>
    /// Creates a failure error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A failure error.</returns>
    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    /// <summary>
    /// Creates an external error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>An external error.</returns>
    public static Error ExternalError(string code, string description) => new(code, description, ErrorType.ExternalError);

    /// <summary>
    /// Creates a service unavailable error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A service unavailable error.</returns>
    public static Error ServiceUnavailable(string code, string description) => new(code, description, ErrorType.ServiceUnavailable);

    /// <summary>
    /// Creates a timeout error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="description">The error description.</param>
    /// <returns>A timeout error.</returns>
    public static Error Timeout(string code, string description) => new(code, description, ErrorType.Timeout);
}

