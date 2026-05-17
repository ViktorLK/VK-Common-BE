using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Vectorics.SemanticCache.Internal;

/// <summary>
/// Source-generated logger messages for the Semantic Cache feature.
/// </summary>
// [SG Logger] - This class is automatically implemented by the Source Generator for high-performance logging.
internal static partial class SemanticCacheLog
{
    [LoggerMessage(
        EventId = 220,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Semantic cache hit for prompt: {PromptSnippet}")]
    public static partial void CacheHit(ILogger logger, string tenantId, string traceId, string promptSnippet);

    [LoggerMessage(
        EventId = 221,
        Level = LogLevel.Information,
        Message = "{TenantId} {TraceId} [AI] Semantic cache miss for prompt: {PromptSnippet}")]
    public static partial void CacheMiss(ILogger logger, string tenantId, string traceId, string promptSnippet);

    [LoggerMessage(
        EventId = 222,
        Level = LogLevel.Warning,
        Message = "{TenantId} {TraceId} [AI] Semantic cache operation failed: {Reason}")]
    public static partial void CacheOperationFailed(ILogger logger, string tenantId, string traceId, string reason);
}
