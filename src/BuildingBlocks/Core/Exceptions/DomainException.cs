namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Represents an exception that originates from a domain rule violation.
/// Use when a business invariant cannot be expressed via the Result pattern
/// (e.g., in constructors or operators where returning a Result is not feasible).
/// </summary>
public class DomainException : BaseException
{
    #region Constructors

    public DomainException(string code, string message) : base(code, message)
    {
    }

    public DomainException(string code, string message, Exception inner) : base(code, message)
    {
        // Note: inner exception is lost because BaseException signature does not take inner exception.
        // We can add innerException to base later if needed, but for now we match the BaseException primary constructor.
    }

    #endregion
}
