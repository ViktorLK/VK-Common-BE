using System;
using Microsoft.Extensions.Logging;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Pattern.Diagnostics;

/// <summary>
/// LoggerMessage Source Generator extension class for Pattern slice. [OR.01]
/// </summary>
internal static partial class PatternLogs
{
    [LoggerMessage(EventId = 7040, Level = LogLevel.Error, Message = "Failed to get current active patterns")]
    public static partial void LogGetCurrentPatternsError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7041, Level = LogLevel.Error, Message = "Failed to get pattern entity {PatternId}")]
    public static partial void LogGetPatternEntityError(this ILogger logger, Exception ex, string patternId);

    [LoggerMessage(EventId = 7042, Level = LogLevel.Error, Message = "Failed to list pattern entities")]
    public static partial void LogListPatternEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7043, Level = LogLevel.Error, Message = "Failed to create pattern entity {PatternId}")]
    public static partial void LogCreatePatternEntityError(this ILogger logger, Exception ex, string patternId);

    [LoggerMessage(EventId = 7044, Level = LogLevel.Error, Message = "Failed to update pattern entity {PatternId}")]
    public static partial void LogUpdatePatternEntityError(this ILogger logger, Exception ex, string patternId);

    [LoggerMessage(EventId = 7045, Level = LogLevel.Error, Message = "Failed to delete pattern entity {PatternId}")]
    public static partial void LogDeletePatternEntityError(this ILogger logger, Exception ex, string patternId);
}
