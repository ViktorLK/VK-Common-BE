namespace VK.Blocks.Core.Results;

/// <summary>
/// Represents a standardized error with a code and description.
/// </summary>
/// <param name="Code">The unique error code.</param>
/// <param name="Description">The error description.</param>
/// <param name="Type">The error type (e.g., Validation, NotFound, Failure).</param>
public record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
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
    /// <summary>A general failure.</summary>
    Failure = 0,

    /// <summary>A validation error.</summary>
    Validation = 1,

    /// <summary>A not found error.</summary>
    NotFound = 2,

    /// <summary>A conflict error.</summary>
    Conflict = 3,

    /// <summary>An unauthorized error.</summary>
    Unauthorized = 4,

    /// <summary>A forbidden error.</summary>
    Forbidden = 5
}
