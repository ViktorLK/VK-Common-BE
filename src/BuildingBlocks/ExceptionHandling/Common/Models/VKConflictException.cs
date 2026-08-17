using System;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Represents a concurrency or state conflict failure.
/// </summary>
public sealed class VKConflictException : VKBusinessException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKConflictException"/> class.
    /// </summary>
    public VKConflictException(string errorCode, string message, Exception? innerException = null)
        : base(errorCode, message, VKErrorType.Conflict, innerException)
    {
    }
}
