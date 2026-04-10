namespace VK.Blocks.Core.Results;

/// <summary>
/// Represents a standardized error with a code and description.
/// </summary>
/// <param name="Code">The unique error code.</param>
/// <param name="Description">The error description.</param>
/// <param name="Type">The error type (e.g., Validation, NotFound, Failure).</param>
public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    #region Fields

    /// <summary>
    /// Represents no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

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
    /// <summary>A general failure. (HTTP 500)</summary>
    Failure = 0,

    /// <summary>A validation error. (HTTP 400)</summary>
    Validation = 1,

    /// <summary>A not found error. (HTTP 404)</summary>
    NotFound = 2,

    /// <summary>A conflict error. (HTTP 409)</summary>
    Conflict = 3,

    /// <summary>An unauthorized error. (HTTP 401)</summary>
    Unauthorized = 4,

    /// <summary>A forbidden error. (HTTP 403)</summary>
    Forbidden = 5,

    /// <summary>Too many requests (Rate limiting). (HTTP 429)</summary>
    TooManyRequests = 6,

    /// <summary>The service is temporarily unavailable. (HTTP 503)</summary>
    ServiceUnavailable = 7,

    /// <summary>A timeout occurred. (HTTP 408/504)</summary>
    Timeout = 8,

    /// <summary>An external service/gateway error. (HTTP 502/504)</summary>
    ExternalError = 9
}
