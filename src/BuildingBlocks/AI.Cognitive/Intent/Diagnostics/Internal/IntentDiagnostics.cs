using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;
using VK.Blocks.AI.Cognitive;

namespace VK.Blocks.AI.Cognitive.Intent.Diagnostics.Internal;

/// <summary>
/// Source-generated logger messages for the Intent extraction layer.
/// Follows OR.01 ([LoggerMessage] SG only).
/// </summary>
[VKBlockDiagnostics<VKAICognitiveBlock>]
internal static partial class IntentDiagnostics
{
    [LoggerMessage(
        EventId = VKIntentDiagnosticTokens.IntentExtractedEventId,
        Level = LogLevel.Debug,
        Message = "Intent extracted: {Intent} (Confidence={Confidence}, Source={Source}, ElapsedMs={ElapsedMs})")]
    public static partial void IntentExtracted(ILogger logger, string intent, double confidence, string source, long elapsedMs);

    [LoggerMessage(
        EventId = VKIntentDiagnosticTokens.IntentExtractionTimeoutEventId,
        Level = LogLevel.Warning,
        Message = "Intent extraction SLA timeout after {TimeoutMs}ms, falling back to heuristic")]
    public static partial void IntentExtractionTimeout(ILogger logger, int timeoutMs);

    [LoggerMessage(
        EventId = VKIntentDiagnosticTokens.CircuitBreakerTrippedEventId,
        Level = LogLevel.Warning,
        Message = "Intent LLM circuit breaker tripped after {FailureCount} consecutive failures")]
    public static partial void CircuitBreakerTripped(ILogger logger, int failureCount);

    [LoggerMessage(
        EventId = VKIntentDiagnosticTokens.CircuitBreakerHalfOpenEventId,
        Level = LogLevel.Information,
        Message = "Intent LLM circuit breaker entering half-open state")]
    public static partial void CircuitBreakerHalfOpen(ILogger logger);

    [LoggerMessage(
        EventId = VKIntentDiagnosticTokens.StructuredOutputFallbackEventId,
        Level = LogLevel.Debug,
        Message = "Structured output not supported by provider, falling back to text parsing")]
    public static partial void StructuredOutputFallback(ILogger logger);

    [LoggerMessage(
        EventId = VKIntentDiagnosticTokens.IntentExtractionFailedEventId,
        Level = LogLevel.Warning,
        Message = "Intent extraction failed for input: {InputPrefix}")]
    public static partial void IntentExtractionFailed(ILogger logger, Exception ex, string inputPrefix);

    [LoggerMessage(
        EventId = VKIntentDiagnosticTokens.IntentParsingFailedEventId,
        Level = LogLevel.Warning,
        Message = "Failed to parse intent LLM response: {ResponsePrefix}")]
    public static partial void IntentParsingFailed(ILogger logger, string responsePrefix);
}
