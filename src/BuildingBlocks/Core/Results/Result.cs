using System;
using System.Collections.Generic;
using System.Linq;

namespace VK.Blocks.Core.Results;

/// <summary>
/// Represents the result of an operation, indicating success or failure.
/// </summary>
public class Result : IResult
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public virtual bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public virtual bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the errors associated with the result.
    /// </summary>
    public Error[] Errors { get; }

    /// <summary>
    /// Gets the primary error associated with the result.
    /// </summary>
    public Error FirstError => Errors.Length > 0 ? Errors[0] : Error.None;

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
        Errors = isSuccess ? [] : [error];
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
        Errors = isSuccess ? [] : errorArray;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="Result"/>.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A successful <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

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

    /// <summary>
    /// Creates a failed result with a specific error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);

    /// <summary>
    /// Creates a failed result with a specific error.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failed <see cref="Result{TValue}"/>.</returns>
    public static Result<TValue> Failure<TValue>(IEnumerable<Error> errors) => new(default, false, errors);
}

