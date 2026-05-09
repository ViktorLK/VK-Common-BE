using System;
using VK.Blocks.Core;

namespace VK.Blocks.ExceptionHandling;

/// <summary>
/// Provides context for the exception being handled, independent of the transport layer.
/// </summary>
/// <param name="Exception">The exception that occurred.</param>
public sealed record VKExceptionContext(Exception Exception)
{
    /// <summary>
    /// Gets or sets the specialized error response resulting from the handling process.
    /// </summary>
    public VKErrorResponse? ErrorResponse { get; init; }

    /// <summary>
    /// Gets or sets the trace identifier associated with the request (optional).
    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the exception has been handled.
    /// </summary>
    public bool Handled { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the exception is pre-categorized as a validation error.
    /// </summary>
    public bool IsValidation { get; init; }
}
