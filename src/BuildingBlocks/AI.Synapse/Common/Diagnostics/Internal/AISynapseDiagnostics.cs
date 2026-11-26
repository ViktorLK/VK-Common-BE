using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Common.Diagnostics.Internal;

/// <summary>
/// Diagnostic meters and activity source definitions for AI Synapse.
/// </summary>
[VKBlockDiagnostics<VKAISynapseBlock>]
internal static partial class AISynapseDiagnostics
{
    private static readonly Counter<long> _totalRequests;
    private static readonly Histogram<double> _requestDuration;
    private static readonly Counter<long> _tokensConsumed;
    private static readonly Counter<double> _costCalculated;
    private static readonly Counter<long> _providerFailures;

    static AISynapseDiagnostics()
    {
        _totalRequests = Meter.CreateCounter<long>(
            VKAISynapseDiagnosticsConstants.TotalRequests,
            "requests",
            "Total AI Synapse requests handled");

        _requestDuration = Meter.CreateHistogram<double>(
            VKAISynapseDiagnosticsConstants.RequestDuration,
            "ms",
            "Duration of AI Synapse request execution in milliseconds");

        _tokensConsumed = Meter.CreateCounter<long>(
            VKAISynapseDiagnosticsConstants.TokensConsumed,
            "tokens",
            "Total tokens consumed across all AI providers");

        _costCalculated = Meter.CreateCounter<double>(
            VKAISynapseDiagnosticsConstants.CostCalculated,
            "USD",
            "Estimated total cost calculated for AI requests");

        _providerFailures = Meter.CreateCounter<long>(
            VKAISynapseDiagnosticsConstants.ProviderFailures,
            "failures",
            "Total provider failures encountered");
    }

    internal static Activity? StartActivity(string name) => Source.StartActivity(name);

    internal static void RecordRequest(string provider, string modelId, bool success, double durationMs)
    {
        var tags = new TagList
        {
            { "provider", provider },
            { "model", modelId },
            { "success", success }
        };

        _totalRequests.Add(1, tags);
        _requestDuration.Record(durationMs, tags);
    }

    internal static void RecordTokensAndCost(string provider, string modelId, long tokens, double cost)
    {
        var tags = new TagList
        {
            { "provider", provider },
            { "model", modelId }
        };

        if (tokens > 0)
        {
            _tokensConsumed.Add(tokens, tags);
        }

        if (cost > 0)
        {
            _costCalculated.Add(cost, tags);
        }
    }

    internal static void RecordProviderFailure(string provider, string modelId, string reason)
    {
        _providerFailures.Add(1, new TagList
        {
            { "provider", provider },
            { "model", modelId },
            { "reason", reason }
        });
    }

    // --- Merged from AISynapseLogs ---
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
