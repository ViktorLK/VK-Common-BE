using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Cognitive.Presence.Internal;

/// <summary>
/// Source-generated logger messages for the Core Presence feature.
/// Follows OR.01.
/// </summary>
internal static partial class PresenceLog
{
    [LoggerMessage(
        EventId = 250,
        Level = LogLevel.Information,
        Message = "Core presence state captured for Session: {SessionId}. Time: {CurrentTime}, Tokens Used: {TotalTokens}")]
    public static partial void PresenceStateCaptured(this ILogger logger, string sessionId, DateTimeOffset currentTime, int totalTokens);

    [LoggerMessage(
        EventId = 251,
        Level = LogLevel.Information,
        Message = "Token usage recorded for Session: {SessionId}. Prompt: +{PromptTokens}, Completion: +{CompletionTokens}")]
    public static partial void TokenUsageRecorded(this ILogger logger, string sessionId, int promptTokens, int completionTokens);

    [LoggerMessage(
        EventId = 252,
        Level = LogLevel.Warning,
        Message = "Presence session not found for Session: {SessionId}. Creating a new one.")]
    public static partial void SessionNotFound(this ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 253,
        Level = LogLevel.Error,
        Message = "Failed to save presence state snapshot for Session: {SessionId}.")]
    public static partial void PresenceStateSaveFailed(this ILogger logger, string sessionId);
}
