using System;

namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Exception thrown when an authenticated user does not have permission to access a resource.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class VKForbiddenException : VKBaseException
{
    private const string DefaultCode = "Core.Forbidden";

    /// <summary>
    /// Gets the name of the permission that was required.
    /// </summary>
    public string? RequiredPermission => Extensions.TryGetValue(nameof(RequiredPermission), out var val) ? val as string : null;

    public VKForbiddenException(string message = "You do not have permission to perform this action.", Exception? innerException = null)
        : base(DefaultCode, message, statusCode: 403, isPublic: true, innerException: innerException)
    {
    }

    public static VKForbiddenException ForPermission(string permission)
    {
        return new VKForbiddenException($"Required permission '{permission}' is missing.")
            .WithExtension(nameof(RequiredPermission), permission);
    }
}
