using System;

namespace VK.Blocks.Core;

/// <summary>
/// Exception thrown when an operation violates the Result pattern contract or accesses invalid Result state.
/// </summary>
public sealed class VKResultException : VKBaseException
{
    private const string DefaultCode = "Core.ResultError";

    /// <summary>
    /// Initializes a new instance of the <see cref="VKResultException"/> class.
    /// </summary>
    /// <param name="message">The human-readable message describing the error.</param>
    /// <param name="innerException">Optional inner exception.</param>
    public VKResultException(string message, Exception? innerException = null)
        : base(DefaultCode, message, statusCode: 500, isPublic: false, innerException: innerException)
    {
    }

    /// <summary>
    /// Creates an exception when accessing the Value property of a failed result.
    /// </summary>
    public static VKResultException FailureValueAccess() =>
        new("Cannot access Value on a failed VKResult. Check IsSuccess before accessing Value.");

    /// <summary>
    /// Creates an exception when a success result unexpectedly contains a null value.
    /// </summary>
    public static VKResultException NullSuccessValue() =>
        new("Success result contains null value. This should not happen.");

    /// <summary>
    /// Creates an exception when creating a success result containing errors.
    /// </summary>
    public static VKResultException InvalidSuccessState(string errorCode) =>
        new($"Success result cannot contain errors (ErrorCode: {errorCode}). Use VKResult.Success() instead.");

    /// <summary>
    /// Creates an exception when creating a failure result without errors.
    /// </summary>
    public static VKResultException InvalidFailureState() =>
        new("Failure result must contain at least one error. Use VKResult.Failure(error) instead.");
}
