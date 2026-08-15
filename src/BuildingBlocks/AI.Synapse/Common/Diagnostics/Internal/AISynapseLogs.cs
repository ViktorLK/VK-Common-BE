using Microsoft.Extensions.Logging;

namespace VK.Blocks.AI.Synapse.Diagnostics.Internal;

/// <summary>
/// Source-generated structured logging for AI Synapse.
/// </summary>
internal static partial class AISynapseLogs
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "AI Synapse routed request to Provider: '{Provider}', Model: '{ModelId}', Tenant: '{TenantId}'.")]
    internal static partial void RequestRouted(this ILogger logger, string provider, string modelId, string? tenantId);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Warning,
        Message = "AI Provider '{Provider}' ({ModelId}) failed: '{ErrorMessage}'. Initiating fallback to next candidate.")]
    internal static partial void ProviderFailedFallback(this ILogger logger, string provider, string modelId, string errorMessage);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "AI Provider '{Provider}' ({ModelId}) rate limit reached. Concurrency: {Concurrency}, Permits: {Permits}.")]
    internal static partial void RateLimitExceeded(this ILogger logger, string provider, string modelId, int concurrency, int permits);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Information,
        Message = "AI request completed. Duration: {DurationMs}ms, Tokens: {Tokens}, Cost: ${Cost:F6}.")]
    internal static partial void RequestCompleted(this ILogger logger, double durationMs, long tokens, double cost);
}
