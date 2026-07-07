using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.AI.Afferent;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Afferent.IngressGuardrails.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIAfferentBlock>]
internal static partial class IngressGuardrailsDiagnostics
{
    [LoggerMessage(
        EventId = VKIngressGuardrailsDiagnosticTokens.GuardrailsPipelineStartedEventId,
        Level = LogLevel.Information,
        Message = "Ingress Guardrails stage initiated for TenantId: {TenantId}, UserId: {UserId}.")]
    public static partial void GuardrailsPipelineStarted(ILogger logger, string tenantId, string userId);

    [LoggerMessage(
        EventId = VKIngressGuardrailsDiagnosticTokens.GuardrailsPipelineCompletedEventId,
        Level = LogLevel.Information,
        Message = "Ingress Guardrails stage successfully completed.")]
    public static partial void GuardrailsPipelineCompleted(ILogger logger);

    [LoggerMessage(
        EventId = VKIngressGuardrailsDiagnosticTokens.GuardrailsViolationDetectedEventId,
        Level = LogLevel.Warning,
        Message = "Ingress Guardrails violation detected! Type: {ViolationType}, Reason: {Reason}")]
    public static partial void GuardrailsViolationDetected(ILogger logger, string violationType, string reason);
}
