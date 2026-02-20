namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Represents an exception that originates from a domain rule violation.
/// Use when a business invariant cannot be expressed via the Result pattern
/// (e.g., in constructors or operators where returning a Result is not feasible).
/// </summary>
public class DomainException : BaseException
{
    #region Constructors

    public DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    public DomainException(string code, string message, Exception inner) : base(message, inner)
    {
        Code = code;
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public override string Code { get; }

    #endregion
}
