using System;
using Microsoft.Extensions.Logging;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Tokenics.Diagnostics.Internal;

[VKBlockDiagnostics<VKAIBlock>]
internal static partial class TokenicsDiagnostics
{
    [LoggerMessage(
        EventId = VKTokenicsDiagnosticsConstants.QuotaExceededEventId,
        Level = LogLevel.Warning,
        Message = "Token quota exceeded. Current: {Current}, Estimated: {Estimated}, Limit: {Limit}")]
    public static partial void QuotaExceeded(ILogger logger, long current, long estimated, long limit);

    [LoggerMessage(
        EventId = VKTokenicsDiagnosticsConstants.TokenCalculationPerformedEventId,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Token calculation performed for model: {Model}")]
    public static partial void TokenCalculationPerformed(ILogger logger, string tenantId, string traceId, string? model);

    [LoggerMessage(
        EventId = VKTokenicsDiagnosticsConstants.TokenicsOperationFailedEventId,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Tokenics operation failed: {Reason}")]
    public static partial void TokenicsOperationFailed(ILogger logger, string tenantId, string traceId, string reason);
}
