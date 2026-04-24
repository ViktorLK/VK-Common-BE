using System.Collections.Generic;

namespace VK.Blocks.Core;

/// <summary>
/// A transport-neutral representation of diagnostic debug information.
/// </summary>
public sealed record VKErrorDebugInfo
{
    /// <summary>
    /// Gets the debug message or the exception message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the fully qualified type name of the underlying error, exception, or event.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Gets the stack trace of the exception or execution stack.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    /// Gets the details of the inner error, if one exists.
    /// </summary>
    public VKErrorDebugInfo? InnerError { get; init; }

    /// <summary>
    /// Gets additional diagnostic or context information.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; init; }
}
