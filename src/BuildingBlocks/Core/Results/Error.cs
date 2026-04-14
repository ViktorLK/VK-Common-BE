namespace VK.Blocks.Core.Results;

/// <summary>
/// Represents a standardized error with a code and description.
/// </summary>
/// <param name="Code">The unique error code.</param>
/// <param name="Description">The error description.</param>
/// <param name="Type">The error type (e.g., Validation, NotFound, Failure).</param>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    #region Factory Methods

    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
    public static Error PreconditionFailed(string code, string description) => new(code, description, ErrorType.PreconditionFailed);
    public static Error TooManyRequests(string code, string description) => new(code, description, ErrorType.TooManyRequests);
    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
    public static Error ExternalError(string code, string description) => new(code, description, ErrorType.ExternalError);
    public static Error ServiceUnavailable(string code, string description) => new(code, description, ErrorType.ServiceUnavailable);
    public static Error Timeout(string code, string description) => new(code, description, ErrorType.Timeout);

    #endregion

    #region Fields

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

    #endregion
}

/// <summary>
/// Defines the types of errors that can occur.
/// </summary>
public enum ErrorType
{
    /// <summary>Represents no error.</summary>
    None = -1,

    /// <summary>A validation error.</summary>
    Validation = 1,

    /// <summary>An unauthorized error.</summary>
    Unauthorized = 4,

    /// <summary>A forbidden error.</summary>
    Forbidden = 5,

    /// <summary>A not found error.</summary>
    NotFound = 2,

    /// <summary>A conflict error.</summary>
    Conflict = 3,

    /// <summary>A precondition failed error.</summary>
    PreconditionFailed = 10,

    /// <summary>Too many requests (Rate limiting).</summary>
    TooManyRequests = 6,

    /// <summary>A general failure.</summary>
    Failure = 0,

    /// <summary>An external service/gateway error.</summary>
    ExternalError = 9,

    /// <summary>The service is temporarily unavailable.</summary>
    ServiceUnavailable = 7,

    /// <summary>A timeout occurred.</summary>
    Timeout = 8
}
