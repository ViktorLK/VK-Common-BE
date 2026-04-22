using System;

namespace VK.Blocks.Core.Exceptions;

/// <summary>
/// Exception thrown when a user is not authenticated.
/// Maps to HTTP 401 Unauthorized.
/// </summary>
public sealed class VKUnauthorizedException : VKBaseException
{
    private const string DefaultCode = "Core.Unauthorized";

    public VKUnauthorizedException(string message = "Authentication is required to access this resource.", Exception? innerException = null)
        : base(DefaultCode, message, statusCode: 401, isPublic: true, innerException: innerException)
    {
    }
}
