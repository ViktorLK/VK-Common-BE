using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore.Profile.Internal;

[ExcludeFromCodeCoverage(Justification = "Source-generated diagnostics logger declarations containing no business logic.")]
[VKBlockDiagnostics<VKAIPsycheEFCoreBlock>]
internal static partial class ProfileDiagnostics
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to get profile entity for ProfileId: {ProfileId}")]
    public static partial void LogGetProfileEntityError(this ILogger logger, Exception ex, string profileId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to list profile entities")]
    public static partial void LogListProfileEntitiesError(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to create profile entity for ProfileId: {ProfileId}")]
    public static partial void LogCreateProfileEntityError(this ILogger logger, Exception ex, string profileId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Failed to update profile entity for ProfileId: {ProfileId}")]
    public static partial void LogUpdateProfileEntityError(this ILogger logger, Exception ex, string profileId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Failed to delete profile entity for ProfileId: {ProfileId}")]
    public static partial void LogDeleteProfileEntityError(this ILogger logger, Exception ex, string profileId);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Failed to get profile in PsycheProfileStore for ProfileId: {ProfileId}")]
    public static partial void LogGetProfileError(this ILogger logger, Exception ex, string profileId);
}
