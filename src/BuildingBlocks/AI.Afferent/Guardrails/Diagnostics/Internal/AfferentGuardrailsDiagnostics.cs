using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Afferent;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.Guardrails.Diagnostics.Internal;

/// <summary>
/// Source-generated high-performance loggers and diagnostics for the Guardrails slice.
/// Follows OR.01.
/// </summary>
[VKBlockDiagnostics<VKAIAfferentBlock>]
internal static partial class AfferentGuardrailsDiagnostics
{
    [LoggerMessage(
        EventId = VKAfferentGuardrailsDiagnosticTokens.GuardrailsPipelineStartedEventId,
        Level = LogLevel.Information,
        Message = "Afferent Guardrails stage initiated for TenantId: {TenantId}, UserId: {UserId}.")]
    public static partial void GuardrailsPipelineStarted(ILogger logger, string tenantId, string userId);

    [LoggerMessage(
        EventId = VKAfferentGuardrailsDiagnosticTokens.GuardrailsPipelineCompletedEventId,
        Level = LogLevel.Information,
        Message = "Afferent Guardrails stage successfully completed.")]
    public static partial void GuardrailsPipelineCompleted(ILogger logger);

    [LoggerMessage(
        EventId = VKAfferentGuardrailsDiagnosticTokens.GuardrailsViolationDetectedEventId,
        Level = LogLevel.Warning,
        Message = "Afferent Guardrails violation detected! Type: {ViolationType}, Reason: {Reason}")]
    public static partial void GuardrailsViolationDetected(ILogger logger, string violationType, string reason);
}
