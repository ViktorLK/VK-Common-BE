using System;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Base exception class for all VK building block exceptions.
/// </summary>
public abstract class VKExceptionBase : Exception
{
    protected VKExceptionBase(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
