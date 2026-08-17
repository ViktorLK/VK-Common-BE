using System;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Represents a business/domain logic exception (expected failure).
/// </summary>
public class VKBusinessException : VKExceptionBase
{
    /// <summary>
    /// Gets the unique error code.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the type of the error.
    /// </summary>
    public VKErrorType ErrorType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VKBusinessException"/> class.
    /// </summary>
    public VKBusinessException(string errorCode, string message, VKErrorType errorType = VKErrorType.Failure, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = VKGuard.NotNullOrWhiteSpace(errorCode);
        ErrorType = errorType;
    }
}
