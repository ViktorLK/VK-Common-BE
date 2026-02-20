using System.Text.Json.Serialization;

namespace VK.Blocks.Core.Results;

/// <summary>
/// Represents the result of an operation with a value.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class Result<TValue> : Result
{
    #region Fields

    private readonly TValue? _value;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class.
    /// </summary>
    protected internal Result(TValue? value, bool isSuccess, IEnumerable<Error> errors)
        : base(isSuccess, errors)
    {
        _value = value;
    }

    #endregion

    #region Properties

    /// <summary>Gets the value associated with the result.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TValue? Value => IsSuccess ? _value ??
        throw new InvalidOperationException("Success result contains null value. This should not happen.")
        : throw new InvalidOperationException("Cannot access Value on a failed Result. Check IsSuccess before accessing Value.");

    #endregion

    #region Operators

    /// <summary>Implicitly converts a value to a success result.</summary>
    public static implicit operator Result<TValue>(TValue? value) => Create(value);

    /// <summary>Implicitly converts an error to a failure result.</summary>
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

    #endregion
}
