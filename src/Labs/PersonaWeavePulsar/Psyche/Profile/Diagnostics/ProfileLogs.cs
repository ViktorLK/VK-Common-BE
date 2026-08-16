using System;
using Microsoft.Extensions.Logging;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Profile.Diagnostics;

/// <summary>
/// LoggerMessage Source Generator extension class for Profile slice. [OR.01]
/// </summary>
internal static partial class ProfileLogs
{
    [LoggerMessage(EventId = 7090, Level = LogLevel.Error, Message = "Failed to get profile for user {UserId}")]
    public static partial void LogGetProfileError(this ILogger logger, Exception ex, string userId);
}
