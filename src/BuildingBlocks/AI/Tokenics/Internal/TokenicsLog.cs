using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Tokenics.Internal;

/// <summary>
/// Source-generated logger messages for the Tokenics feature.
/// </summary>
// [SG Logger] - This class is automatically implemented by the Source Generator for high-performance logging.
internal static partial class TokenicsLog
{
    [LoggerMessage(
        EventId = 400,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Token calculation performed for model: {Model}")]
    public static partial void TokenCalculationPerformed(ILogger logger, string tenantId, string traceId, string? model);

    [LoggerMessage(
        EventId = 401,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Tokenics operation failed: {Reason}")]
    public static partial void TokenicsOperationFailed(ILogger logger, string tenantId, string traceId, string reason);
}
