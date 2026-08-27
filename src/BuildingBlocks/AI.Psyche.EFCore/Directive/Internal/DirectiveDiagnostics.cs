using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Directive.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class DirectiveDiagnostics
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to get directive entity for DirectiveId: {DirectiveId}")]
    public static partial void LogGetDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to list directive entities")]
    public static partial void LogListDirectiveEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to create directive entity for DirectiveId: {DirectiveId}")]
    public static partial void LogCreateDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Failed to update directive entity for DirectiveId: {DirectiveId}")]
    public static partial void LogUpdateDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Failed to delete directive entity for DirectiveId: {DirectiveId}")]
    public static partial void LogDeleteDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Failed to get directives in PsycheDirectiveStore for IDs: {DirectiveIds}")]
    public static partial void LogGetDirectivesStoreError(this ILogger logger, Exception ex, string directiveIds);
}
