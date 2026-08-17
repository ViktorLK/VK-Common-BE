using System;
using Microsoft.Extensions.Logging;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona.Diagnostics;

/// <summary>
/// LoggerMessage Source Generator extension class for Persona slice. [OR.01]
/// </summary>
internal static partial class PersonaLogs
{
    [LoggerMessage(EventId = 7050, Level = LogLevel.Error, Message = "Failed to get persona anchor {PersonaId}")]
    public static partial void LogGetPersonaError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 7051, Level = LogLevel.Error, Message = "Failed to get persona entity {PersonaId}")]
    public static partial void LogGetPersonaEntityError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 7052, Level = LogLevel.Error, Message = "Failed to list all persona entities")]
    public static partial void LogListPersonaEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7053, Level = LogLevel.Error, Message = "Failed to create persona entity {PersonaId}")]
    public static partial void LogCreatePersonaEntityError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 7054, Level = LogLevel.Error, Message = "Failed to update persona entity {PersonaId}")]
    public static partial void LogUpdatePersonaEntityError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 7055, Level = LogLevel.Error, Message = "Failed to delete persona entity {PersonaId}")]
    public static partial void LogDeletePersonaEntityError(this ILogger logger, Exception ex, string personaId);
}
