using System;
using Microsoft.Extensions.Logging;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Knowledge.Diagnostics;

/// <summary>
/// LoggerMessage Source Generator extension class for Knowledge slice. [OR.01]
/// </summary>
internal static partial class KnowledgeLogs
{
    [LoggerMessage(EventId = 7020, Level = LogLevel.Error, Message = "Failed to get relevant knowledge entries for persona {PersonaId}")]
    public static partial void LogGetRelevantKnowledgeError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 7021, Level = LogLevel.Error, Message = "Failed to get knowledge entity {KnowledgeId}")]
    public static partial void LogGetKnowledgeEntityError(this ILogger logger, Exception ex, string knowledgeId);

    [LoggerMessage(EventId = 7022, Level = LogLevel.Error, Message = "Failed to list knowledge entities")]
    public static partial void LogListKnowledgeEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7023, Level = LogLevel.Error, Message = "Failed to create knowledge entity {EntryId}")]
    public static partial void LogCreateKnowledgeEntityError(this ILogger logger, Exception ex, string entryId);

    [LoggerMessage(EventId = 7024, Level = LogLevel.Error, Message = "Failed to update knowledge entity {EntryId}")]
    public static partial void LogUpdateKnowledgeEntityError(this ILogger logger, Exception ex, string entryId);

    [LoggerMessage(EventId = 7025, Level = LogLevel.Error, Message = "Failed to delete knowledge entity {EntryId}")]
    public static partial void LogDeleteKnowledgeEntityError(this ILogger logger, Exception ex, string entryId);

    [LoggerMessage(EventId = 7026, Level = LogLevel.Error, Message = "Failed to get knowledge books")]
    public static partial void LogGetBooksError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 7027, Level = LogLevel.Error, Message = "Failed to create knowledge book {BookId}")]
    public static partial void LogCreateBookError(this ILogger logger, Exception ex, string bookId);

    [LoggerMessage(EventId = 7028, Level = LogLevel.Error, Message = "Failed to update knowledge book {BookId}")]
    public static partial void LogUpdateBookError(this ILogger logger, Exception ex, string bookId);

    [LoggerMessage(EventId = 7029, Level = LogLevel.Error, Message = "Failed to delete knowledge book {BookId}")]
    public static partial void LogDeleteBookError(this ILogger logger, Exception ex, string bookId);

    [LoggerMessage(EventId = 7030, Level = LogLevel.Error, Message = "Failed to get book IDs for persona {PersonaId}")]
    public static partial void LogGetPersonaBookIdsError(this ILogger logger, Exception ex, string personaId);

    [LoggerMessage(EventId = 7031, Level = LogLevel.Error, Message = "Failed to set book IDs for persona {PersonaId}")]
    public static partial void LogSetPersonaBooksError(this ILogger logger, Exception ex, string personaId);
}
