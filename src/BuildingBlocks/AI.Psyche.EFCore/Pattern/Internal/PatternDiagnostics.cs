using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Pattern.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class PatternDiagnostics
{
    [LoggerMessage(EventId = 73301, Level = LogLevel.Error, Message = "Failed to get pattern entity for PatternId: {PatternId}")]
    public static partial void LogGetPatternEntityError(this ILogger logger, Exception ex, string patternId);

    [LoggerMessage(EventId = 73302, Level = LogLevel.Error, Message = "Failed to list pattern entities")]
    public static partial void LogListPatternEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 73303, Level = LogLevel.Error, Message = "Failed to create pattern entity for PatternId: {PatternId}")]
    public static partial void LogCreatePatternEntityError(this ILogger logger, Exception ex, string patternId);

    [LoggerMessage(EventId = 73304, Level = LogLevel.Error, Message = "Failed to update pattern entity for PatternId: {PatternId}")]
    public static partial void LogUpdatePatternEntityError(this ILogger logger, Exception ex, string patternId);

    [LoggerMessage(EventId = 73305, Level = LogLevel.Error, Message = "Failed to delete pattern entity for PatternId: {PatternId}")]
    public static partial void LogDeletePatternEntityError(this ILogger logger, Exception ex, string patternId);

    [LoggerMessage(EventId = 73306, Level = LogLevel.Error, Message = "Failed to get patterns in PsychePatternStore for IDs: {PatternIds}")]
    public static partial void LogGetPatternsStoreError(this ILogger logger, Exception ex, string patternIds);

    [VKMetricHistogram("vk.ai.psyche.efcore.pattern.duration", Unit = "ms", Description = "Duration of EFCore Pattern database operations in milliseconds.")]
    public static partial void RecordPatternOperation(double durationMs, [VKMetricTag("operation")] string operation, [VKMetricTag("success")] bool success);

    [VKMetricCounter("vk.ai.psyche.efcore.pattern.errors", Unit = "errors", Description = "Total number of EFCore Pattern database errors.")]
    public static partial void RecordPatternError([VKMetricTag("operation")] string operation);
}
