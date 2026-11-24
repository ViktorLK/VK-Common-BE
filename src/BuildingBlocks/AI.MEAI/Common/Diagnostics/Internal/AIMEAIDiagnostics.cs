using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI.Common.Diagnostics.Internal;

// [SG Diagnostics]
[VKBlockDiagnostics<VKAIMEAIBlock>]
internal static partial class AIMEAIDiagnostics
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "AI Chat response completed in {DurationMs}ms. Model: {Model}. PromptTokens: {PromptTokens}, CompletionTokens: {CompletionTokens}")]
    public static partial void ChatCompleted(this ILogger logger, double durationMs, string? model, long promptTokens, long completionTokens);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "AI Chat request failed after {DurationMs}ms.")]
    public static partial void ChatFailed(this ILogger logger, Exception ex, double durationMs);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "AI Chat streaming completed in {DurationMs}ms.")]
    public static partial void StreamingCompleted(this ILogger logger, double durationMs);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "AI Chat streaming initialization failed.")]
    public static partial void StreamingInitFailed(this ILogger logger, Exception ex);

    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Prompt injection attempt detected. Type: {PatternType}, Confidence: {ConfidenceScore}")]
    public static partial void PromptInjectionDetected(this ILogger logger, string patternType, float confidenceScore);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Token governance applied. OriginalTokens: {OriginalTokens}, PrunedTokens: {PrunedTokens}")]
    public static partial void TokenGovernanceApplied(this ILogger logger, int originalTokens, int prunedTokens);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "AI Embedding generation completed in {DurationMs}ms. Count: {ItemCount}, VectorDimensions: {Dimensions}")]
    public static partial void EmbeddingCompleted(this ILogger logger, double durationMs, int itemCount, int dimensions);

    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "AI Embedding generation failed after {DurationMs}ms.")]
    public static partial void EmbeddingFailed(this ILogger logger, Exception ex, double durationMs);

    [LoggerMessage(EventId = 9, Level = LogLevel.Error, Message = "AI Speech streaming failed.")]
    public static partial void SpeechStreamingFailed(this ILogger logger, Exception ex);
}
