using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Profile.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class ProfileDiagnostics
{
    [LoggerMessage(EventId = 73601, Level = LogLevel.Error, Message = "Failed to get profile entity for ProfileId: {ProfileId}")]
    public static partial void LogGetProfileEntityError(this ILogger logger, Exception ex, string profileId);

    [LoggerMessage(EventId = 73602, Level = LogLevel.Error, Message = "Failed to list profile entities")]
    public static partial void LogListProfileEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 73603, Level = LogLevel.Error, Message = "Failed to create profile entity for ProfileId: {ProfileId}")]
    public static partial void LogCreateProfileEntityError(this ILogger logger, Exception ex, string profileId);

    [LoggerMessage(EventId = 73604, Level = LogLevel.Error, Message = "Failed to update profile entity for ProfileId: {ProfileId}")]
    public static partial void LogUpdateProfileEntityError(this ILogger logger, Exception ex, string profileId);

    [LoggerMessage(EventId = 73605, Level = LogLevel.Error, Message = "Failed to delete profile entity for ProfileId: {ProfileId}")]
    public static partial void LogDeleteProfileEntityError(this ILogger logger, Exception ex, string profileId);

    [LoggerMessage(EventId = 73606, Level = LogLevel.Error, Message = "Failed to get profile in PsycheProfileStore for ProfileId: {ProfileId}")]
    public static partial void LogGetProfileError(this ILogger logger, Exception ex, string profileId);

    [VKMetricHistogram("vk.ai.psyche.efcore.profile.duration", Unit = "ms", Description = "Duration of EFCore Profile database operations in milliseconds.")]
    public static partial void RecordProfileOperation(double durationMs, [VKMetricTag("operation")] string operation, [VKMetricTag("success")] bool success);

    [VKMetricCounter("vk.ai.psyche.efcore.profile.errors", Unit = "errors", Description = "Total number of EFCore Profile database errors.")]
    public static partial void RecordProfileError([VKMetricTag("operation")] string operation);
}
