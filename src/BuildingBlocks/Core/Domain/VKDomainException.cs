using System;

namespace VK.Blocks.Core;

/// <summary>
/// Represents an exception that originates from a domain rule violation.
/// Use when a business invariant cannot be expressed via the VKResult pattern
/// (e.g., in constructors or operators where returning a VKResult is not feasible).
/// </summary>
public sealed class VKDomainException : VKBaseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKDomainException"/> class.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public VKDomainException(string code, string message)
        : base(code, message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKDomainException"/> class with an inner exception.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public VKDomainException(string code, string message, Exception inner)
        : base(code, message, innerException: inner)
    {
    }
}
