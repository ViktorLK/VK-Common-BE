using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace VK.Blocks.Core;

/// <summary>
/// Represents the result of an operation, indicating success or failure.
/// </summary>
public class VKResult : IVKResult
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
    public VKError[] Errors { get; }
    public VKError FirstError => Errors.Length > 0 ? Errors[0] : VKError.None;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKResult"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error associated with the result.</param>
    protected VKResult(bool isSuccess, VKError error)
    {
        bool hasError = error != VKError.None;
        if (isSuccess && hasError)
        {
            ThrowInvalidSuccessState(error.Code);
        }

        if (!isSuccess && !hasError)
        {
            ThrowInvalidFailureState();
        }

        IsSuccess = isSuccess;
        Errors = isSuccess ? [] : [error];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKResult"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="errors">The errors associated with the result.</param>
    protected VKResult(bool isSuccess, IEnumerable<VKError> errors)
    {
        // Avoid LINQ allocations in a high-frequency constructor
        VKError[] errorArray;
        bool hasError = false;
        string? firstErrorCode = null;

        if (errors is null)
        {
            errorArray = [];
        }
        else if (errors is VKError[] arr)
        {
            errorArray = arr;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != VKError.None)
                {
                    hasError = true;
                    firstErrorCode ??= arr[i].Code;
                    break;
                }
            }
        }
        else
        {
            List<VKError> list = [];
            foreach (VKError error in errors)
            {
                list.Add(error);
                if (error != VKError.None)
                {
                    hasError = true;
                    firstErrorCode ??= error.Code;
                }
            }
            errorArray = [.. list];
        }

        if (isSuccess && hasError)
        {
            ThrowInvalidSuccessState(firstErrorCode ?? "Unknown");
        }

        if (!isSuccess && !hasError)
        {
            ThrowInvalidFailureState();
        }

        IsSuccess = isSuccess;
        Errors = isSuccess ? [] : errorArray;
    }

    private static readonly VKResult _success = new(true, VKError.None);

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful <see cref="VKResult"/>.</returns>
    public static VKResult Success() => _success;

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A successful <see cref="VKResult{TValue}"/>.</returns>
    public static VKResult<TValue> Success<TValue>(TValue value) => new(value, true, VKError.None);

    /// <summary>
    /// Creates a result based on a value.
    /// By design, this method enforces strict null-safety: if the value is not null, it returns a successful result;
    /// if the value is null, it intentionally returns a failure result with <see cref="VKError.NullValue"/>.
    /// This guarantees that a successful <see cref="VKResult{TValue}"/> will never contain a null value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to evaluate.</param>
    /// <returns>A success result if the value is not null; otherwise, a failure result.</returns>
    public static VKResult<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(VKError.NullValue);

    /// <summary>
    /// Creates a failed result with a specific error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="VKResult"/>.</returns>
    public static VKResult Failure(VKError error) => new(false, error);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failed <see cref="VKResult"/>.</returns>
    public static VKResult Failure(IEnumerable<VKError> errors) => new(false, errors);

    /// <summary>
    /// Creates a failed result with a specific error.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A failed <see cref="VKResult{TValue}"/>.</returns>
    public static VKResult<TValue> Failure<TValue>(VKError error) => new(default, false, error);

    /// <summary>
    /// Creates a failed result with multiple errors.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="errors">The collection of errors.</param>
    /// <returns>A failed <see cref="VKResult{TValue}"/>.</returns>
    public static VKResult<TValue> Failure<TValue>(IEnumerable<VKError> errors) => new(default, false, errors);

    [DoesNotReturn]
    private static void ThrowInvalidSuccessState(string errorCode) =>
        throw new InvalidOperationException($"Success result cannot contain errors (ErrorCode: {errorCode}). Use VKResult.Success() instead.");

    [DoesNotReturn]
    private static void ThrowInvalidFailureState() =>
        throw new InvalidOperationException("Failure result must contain at least one error. Use VKResult.Failure(error) instead.");
}
