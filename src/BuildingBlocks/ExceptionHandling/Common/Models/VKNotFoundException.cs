using System;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Represents a resource not found failure.
/// </summary>
public sealed class VKNotFoundException : VKBusinessException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VKNotFoundException"/> class.
    /// </summary>
    public VKNotFoundException(string errorCode, string message, Exception? innerException = null)
        : base(errorCode, message, VKErrorType.NotFound, innerException)
    {
    }
}
