using System;

namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict occurs (e.g., duplicate entry or concurrency failure).
/// Maps to HTTP 409 Conflict.
/// </summary>
public sealed class VKConflictException : VKBaseException
{
    private const string DefaultCode = "Core.Conflict";

    /// <summary>
    /// Gets the reason for the conflict.
    /// </summary>
    public string? Reason => Extensions.TryGetValue(nameof(Reason), out var val) ? val as string : null;

    public VKConflictException(string message, Exception? innerException = null)
        : base(DefaultCode, message, statusCode: 409, isPublic: true, innerException: innerException)
    {
    }

    public static VKConflictException Duplicate(string entityType, string key)
    {
        return new VKConflictException($"Entity '{entityType}' with key '{key}' already exists.")
            .WithExtension(nameof(Reason), "Duplicate")
            .WithExtension("EntityType", entityType)
            .WithExtension("Key", key);
    }
}
