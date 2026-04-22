using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace VK.Blocks.Core;

/// <summary>
/// Represents the result of an operation with a value.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public sealed class VKResult<TValue> : VKResult
{
    private readonly TValue? _value;

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool IsSuccess => base.IsSuccess;

    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(Value))]
    public override bool IsFailure => base.IsFailure;

    /// <summary>
    /// Gets the value associated with the result.
    /// It is guaranteed by design that if <see cref="VKResult.IsSuccess"/> is true, this value will not be null.
    /// Accessing this property on a failure result throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a failure result, or if a success result unexpectedly contains a null value.</exception>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TValue? Value => IsSuccess ? (_value ?? VKResult<TValue>.ThrowNullSuccess()) : VKResult<TValue>.ThrowFailureAccess();

    [DoesNotReturn]
    private static TValue ThrowNullSuccess() =>
        throw new InvalidOperationException("Success result contains null value. This should not happen.");

    [DoesNotReturn]
    private static TValue ThrowFailureAccess() =>
        throw new InvalidOperationException("Cannot access Value on a failed VKResult. Check IsSuccess before accessing Value.");

    /// <summary>
    /// Initializes a new instance of the <see cref="VKResult{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error associated with the result.</param>
    internal VKResult(TValue? value, bool isSuccess, VKError error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKResult{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="errors">The errors associated with the result.</param>
    internal VKResult(TValue? value, bool isSuccess, IEnumerable<VKError> errors)
        : base(isSuccess, errors)
    {
        _value = value;
    }

    /// <summary>
    /// Implicitly converts a value to a success result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator VKResult<TValue>(TValue? value) => Create(value);

    /// <summary>
    /// Implicitly converts an error to a failure result.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    public static implicit operator VKResult<TValue>(VKError error) => Failure<TValue>(error);
}
