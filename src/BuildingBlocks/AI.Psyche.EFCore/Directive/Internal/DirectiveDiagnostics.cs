using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Directive.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class DirectiveDiagnostics
{
    [LoggerMessage(EventId = 73101, Level = LogLevel.Error, Message = "Failed to get directive entity for DirectiveId: {DirectiveId}")]
    public static partial void LogGetDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 73102, Level = LogLevel.Error, Message = "Failed to list directive entities")]
    public static partial void LogListDirectiveEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 73103, Level = LogLevel.Error, Message = "Failed to create directive entity for DirectiveId: {DirectiveId}")]
    public static partial void LogCreateDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 73104, Level = LogLevel.Error, Message = "Failed to update directive entity for DirectiveId: {DirectiveId}")]
    public static partial void LogUpdateDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 73105, Level = LogLevel.Error, Message = "Failed to delete directive entity for DirectiveId: {DirectiveId}")]
    public static partial void LogDeleteDirectiveEntityError(this ILogger logger, Exception ex, string directiveId);

    [LoggerMessage(EventId = 73106, Level = LogLevel.Error, Message = "Failed to get directives in PsycheDirectiveStore for IDs: {DirectiveIds}")]
    public static partial void LogGetDirectivesStoreError(this ILogger logger, Exception ex, string directiveIds);

    [VKMetricHistogram("vk.ai.psyche.efcore.directive.duration", Unit = "ms", Description = "Duration of EFCore Directive database operations in milliseconds.")]
    public static partial void RecordDirectiveOperation(double durationMs, [VKMetricTag("operation")] string operation, [VKMetricTag("success")] bool success);

    [VKMetricCounter("vk.ai.psyche.efcore.directive.errors", Unit = "errors", Description = "Total number of EFCore Directive database errors.")]
    public static partial void RecordDirectiveError([VKMetricTag("operation")] string operation);
}
