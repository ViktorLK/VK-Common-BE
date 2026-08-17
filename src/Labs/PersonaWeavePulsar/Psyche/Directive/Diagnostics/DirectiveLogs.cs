using System;
using Microsoft.Extensions.Logging;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Directive.Diagnostics;

/// <summary>
/// LoggerMessage Source Generator extension class for Directive slice. [OR.01]
/// </summary>
internal static partial class DirectiveLogs
{
    [LoggerMessage(EventId = 7010, Level = LogLevel.Error, Message = "Failed to get directive charter {DirectiveId}")]
    public static partial void LogGetDirectiveError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 7011, Level = LogLevel.Error, Message = "Failed to get directive entity {DirectiveId}")]
    public static partial void LogGetDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 7012, Level = LogLevel.Error, Message = "Failed to list directive entities")]
    public static partial void LogListDirectiveEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7013, Level = LogLevel.Error, Message = "Failed to create directive entity {DirectiveId}")]
    public static partial void LogCreateDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 7014, Level = LogLevel.Error, Message = "Failed to update directive entity {DirectiveId}")]
    public static partial void LogUpdateDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 7015, Level = LogLevel.Error, Message = "Failed to delete directive entity {DirectiveId}")]
    public static partial void LogDeleteDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);
}
