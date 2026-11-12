using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Guardrails.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class GuardrailsDiagnostics
{
    [LoggerMessage(
        EventId = VKGuardrailsDiagnosticsConstants.ContentFlaggedEventId,
        Level = LogLevel.Warning,
        Message = "Content flagged by moderation engine. Reason: {Reason}")]
    public static partial void ContentFlagged(ILogger logger, string? reason);

    [LoggerMessage(
        EventId = VKGuardrailsDiagnosticsConstants.ModerationCheckPerformedEventId,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Moderation check performed for model: {Model}")]
    public static partial void ModerationCheckPerformed(ILogger logger, string tenantId, string traceId, string? model);

    [LoggerMessage(
        EventId = VKGuardrailsDiagnosticsConstants.ModerationRequestFailedEventId,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Moderation request failed: {Reason}")]
    public static partial void ModerationRequestFailed(ILogger logger, string tenantId, string traceId, string reason);
}
