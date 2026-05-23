using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Common.Diagnostics.Internal;

/// <summary>
/// Diagnostics for the AI Cognitive building block.
/// </summary>
[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class VKAICognitiveLog
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "AI Cognitive Block initialized for {BlockName}")]
    internal static partial void LogVKAICognitiveBlockInitialized(this ILogger logger, string blockName);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Cognitive Agent {Name} started with Persona: {PersonaId}")]
    internal static partial void CognitiveAgentStarted(this ILogger logger, string name, string personaId);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Cognitive Agent {Name} is thinking: {Thought}")]
    internal static partial void CognitiveAgentThinking(this ILogger logger, string name, string thought);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Cognitive Agent {Name} encountered an unexpected execution error.")]
    internal static partial void UnexpectedExecutionError(this ILogger logger, string name, System.Exception ex);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Cognitive Pipeline interceptor '{InterceptorType}' failed during background execution of OnAfterChatAsync for Session: {SessionId}")]
    internal static partial void LogInterceptorBackgroundError(this ILogger logger, string interceptorType, string sessionId, System.Exception ex);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Unexpected error occurred while processing Audit Synapse queue.")]
    internal static partial void LogQueueProcessingError(this ILogger logger, System.Exception ex);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "TokenUsage metadata is missing for Session: {SessionId}. Falling back to local text token counting, which may cause severe under-billing for reasoning models.")]
    internal static partial void LogMissingUsageMetadataWarning(this ILogger logger, string sessionId);
}
