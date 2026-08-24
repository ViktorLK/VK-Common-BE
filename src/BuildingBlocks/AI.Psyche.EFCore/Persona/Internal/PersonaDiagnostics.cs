using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Persona.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class PersonaDiagnostics
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to get persona entity for PersonaId: {PersonaId}")]
    public static partial void LogGetPersonaEntityError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to list persona entities")]
    public static partial void LogListPersonaEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to create persona entity for PersonaId: {PersonaId}")]
    public static partial void LogCreatePersonaEntityError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Failed to update persona entity for PersonaId: {PersonaId}")]
    public static partial void LogUpdatePersonaEntityError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Failed to delete persona entity for PersonaId: {PersonaId}")]
    public static partial void LogDeletePersonaEntityError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Failed to get personas in PsychePersonaStore for IDs: {PersonaIds}")]
    public static partial void LogGetPersonaError(this ILogger logger, Exception ex, string personaIds);
}
