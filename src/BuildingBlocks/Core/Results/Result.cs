namespace VK.Blocks.Core.Results;

/// <summary>
/// Represents the result of an operation, indicating success or failure.
/// </summary>
public class Result : IResult
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error associated with the result.</param>
    protected Result(bool isSuccess, Error error)
    {
        var hasError = error != Error.None;
        switch (isSuccess, hasError)
        {
            case (true, true):
                throw new InvalidOperationException($"Invalid Result state: IsSuccess=true but Error={error.Code}. Success results must use Error.None.");
            case (false, false):
                throw new InvalidOperationException($"Invalid Result state: IsSuccess=false but Error=Error.None. Failure results must specify a valid error.");
        }

        IsSuccess = isSuccess;
        Errors = [error];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="errors">The errors associated with the result.</param>
    protected Result(bool isSuccess, IEnumerable<Error> errors)
    {
        var errorArray = errors?.ToArray() ?? [];
        var hasError = errorArray.Any(x => x != Error.None);

        switch (isSuccess, hasError)
        {
            case (true, true):
                throw new InvalidOperationException("Success result cannot contain errors. Use Result.Success() instead.");
            case (false, false):
                throw new InvalidOperationException("Failure result must contain at least one error. Use Result.Failure(error) instead.");
        }

        IsSuccess = isSuccess;
        Errors = errorArray;
    }

    #endregion

    #region Properties

    /// <summary>Gets a value indicating whether the operation was successful.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the errors associated with the result.</summary>
    public Error[] Errors { get; }

    /// <summary>Gets the primary error associated with the result.</summary>
    public Error Error => Errors.Length > 0 ? Errors[0] : Error.None;

    #endregion

    #region Public Methods

    /// <summary>Creates a successful result.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result with a specific error.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a failed result with multiple errors.</summary>
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);

    /// <summary>Creates a successful result with a value.</summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>Creates a failed result with a specific error.</summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>Creates a failed result with multiple errors.</summary>
    public static Result<TValue> Failure<TValue>(IEnumerable<Error> errors) => new(default, false, errors);

    /// <summary>
    /// Creates a result based on a value.
    /// By design, this method enforces strict null-safety: if the value is not null, it returns a successful result;
    /// if the value is null, it intentionally returns a failure result with <see cref="Error.NullValue"/>.
    /// This guarantees that a successful <see cref="Result{TValue}"/> will never contain a null value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to evaluate.</param>
    /// <returns>A success result if the value is not null; otherwise, a failure result.</returns>
    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    #endregion
}
