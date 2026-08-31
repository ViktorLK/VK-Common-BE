using System;

namespace VK.Blocks.Core;

/// <summary>
/// Exception thrown when an operation requires an active execution or tenant/user ambient context, but none is found.
/// </summary>
public sealed class VKContextException : VKBaseException
{
    private const string DefaultCode = "Core.ContextError";

    /// <summary>
    /// Initializes a new instance of the <see cref="VKContextException"/> class.
    /// </summary>
    /// <param name="message">The human-readable message describing the context error.</param>
    /// <param name="innerException">Optional inner exception.</param>
    public VKContextException(string message, Exception? innerException = null)
        : base(DefaultCode, message, statusCode: 500, isPublic: false, innerException: innerException)
    {
    }

    /// <summary>
    /// Creates an exception indicating that an active ambient tenant coordinate is missing.
    /// </summary>
    public static VKContextException MissingTenantCoordinate() =>
        new("Execution requires an active ambient tenant coordinate, but none is set in the active async context.");

    /// <summary>
    /// Creates an exception indicating that an active ambient user coordinate is missing.
    /// </summary>
    public static VKContextException MissingUserCoordinate() =>
        new("Execution requires an active ambient user coordinate, but none is set in the active async context.");
}
