using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Diagnostics.Internal;

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
}
