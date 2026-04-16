namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Represents an exception that originates from a domain rule violation.
/// Use when a business invariant cannot be expressed via the Result pattern
/// (e.g., in constructors or operators where returning a Result is not feasible).
/// </summary>
public sealed class DomainException : BaseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public DomainException(string code, string message)
        : base(code, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class with an inner exception.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public DomainException(string code, string message, Exception inner)
        : base(code, message)
    {
        // Note: inner exception is lost because BaseException signature does not take inner exception.
        // We can add innerException to base later if needed, but for now we match the BaseException primary constructor.
    }
}

