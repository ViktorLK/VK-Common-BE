using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cortex.Common.Diagnostics.Internal;

/// <summary>
/// Structured source-generated diagnostics and metrics for AI.Cortex building block.
/// Follows OR.01, BB.04.
/// </summary>
[VKBlockDiagnostics<VKAICortexBlock>]
internal static partial class CortexDiagnostics
{
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "Turn orchestration started for Session {SessionId} with Trace {TraceId}.")]
    public static partial void TurnOrchestrationStarted(this ILogger logger, string sessionId, string traceId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Turn orchestration completed for Session {SessionId} with Trace {TraceId} in {ElapsedMs}ms. Tokens used: {TokensUsed}.")]
    public static partial void TurnOrchestrationCompleted(this ILogger logger, string sessionId, string traceId, double elapsedMs, long tokensUsed);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Warning,
        Message = "Turn orchestration failed for Session {SessionId} with Trace {TraceId}: {Error}")]
    public static partial void TurnOrchestrationFailed(this ILogger logger, string sessionId, string traceId, string error);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Information,
        Message = "Session {SessionId} boundary triggered. Initiating multi-block consolidation coordination.")]
    public static partial void SessionBoundaryTriggered(this ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Information,
        Message = "Session {SessionId} consolidation completed successfully across Engram, Somatic, and Corpus.")]
    public static partial void SessionConsolidationCompleted(this ILogger logger, string sessionId);

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Error,
        Message = "Session {SessionId} consolidation failed during step {StepName}: {Error}")]
    public static partial void SessionConsolidationStepFailed(this ILogger logger, string sessionId, string stepName, string error);
}
