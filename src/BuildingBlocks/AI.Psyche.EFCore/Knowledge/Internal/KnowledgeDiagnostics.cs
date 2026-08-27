using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Knowledge.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class KnowledgeDiagnostics
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to get knowledge entity for KnowledgeId: {KnowledgeId}")]
    public static partial void LogGetKnowledgeEntityError(this ILogger logger, Exception ex, string knowledgeId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to list knowledge entities")]
    public static partial void LogListKnowledgeEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to create knowledge entity for KnowledgeId: {KnowledgeId}")]
    public static partial void LogCreateKnowledgeEntityError(this ILogger logger, Exception ex, string knowledgeId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Failed to update knowledge entity for KnowledgeId: {KnowledgeId}")]
    public static partial void LogUpdateKnowledgeEntityError(this ILogger logger, Exception ex, string knowledgeId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Failed to delete knowledge entity for KnowledgeId: {KnowledgeId}")]
    public static partial void LogDeleteKnowledgeEntityError(this ILogger logger, Exception ex, string knowledgeId);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Failed to get knowledge entries in PsycheKnowledgeStore for IDs: {KnowledgeIds}")]
    public static partial void LogGetKnowledgeStoreError(this ILogger logger, Exception ex, string knowledgeIds);
}
