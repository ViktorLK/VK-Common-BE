using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Guardrails.Internal;

/// <summary>
/// Source-generated logger messages for the Moderation feature.
/// </summary>
// [SG Logger] - This class is automatically implemented by the Source Generator for high-performance logging.
internal static partial class ModerationLog
{
    [LoggerMessage(
        EventId = 310,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Moderation check performed for model: {Model}")]
    public static partial void ModerationCheckPerformed(ILogger logger, string tenantId, string traceId, string? model);

    [LoggerMessage(
        EventId = 311,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Moderation request failed: {Reason}")]
    public static partial void ModerationRequestFailed(ILogger logger, string tenantId, string traceId, string reason);
}
