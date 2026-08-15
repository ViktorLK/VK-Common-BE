using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Routing.Internal;

// [AP.01] sealed
internal sealed class DefaultAIRouter : IVKAIRouter
{
    private readonly IVKAIProviderTracker _tracker;
    private readonly VKRoutingOptions _options;
    private readonly IVKAIMetricsCollector? _metricsCollector;

    public DefaultAIRouter(
        IVKAIProviderTracker tracker,
        VKRoutingOptions options,
        IVKAIMetricsCollector? metricsCollector = null)
    {
        _tracker = VKGuard.NotNull(tracker);
        _options = VKGuard.NotNull(options);
        _metricsCollector = metricsCollector;
    }

    public Task<VKResult<IReadOnlyList<VKAIConnection>>> ResolveCandidatesAsync(
        VKAIRouteArgs? args,
        IEnumerable<VKAIConnection> pool,
        CancellationToken cancellationToken = default)
    {
        args ??= new VKAIRouteArgs();
        VKGuard.NotNull(pool);

        var candidateList = pool.ToList();
        if (candidateList.Count == 0)
        {
            return Task.FromResult(VKResult.Failure<IReadOnlyList<VKAIConnection>>(VKAISynapseErrors.NoAvailableProvider));
        }

        // 1. Filter only available connections (health & rate limit check)
        var healthy = candidateList
            .Where(p => _tracker.IsAvailable(p))
            .ToList();

        if (healthy.Count == 0)
        {
            return Task.FromResult(VKResult.Failure<IReadOnlyList<VKAIConnection>>(VKAISynapseErrors.NoAvailableProvider));
        }

        // 2. Order based on configured strategy
        IReadOnlyList<VKAIConnection> sorted;
        switch (_options.Strategy)
        {
            case VKAIRoutingStrategy.CostOptimized:
                sorted = healthy.OrderBy(p => GetCostScore(p)).ToList();
                break;

            case VKAIRoutingStrategy.WeightedRoundRobin:
                sorted = healthy.OrderByDescending(p => GetWeightedScore(p)).ToList();
                break;

            case VKAIRoutingStrategy.LatencyOptimized:
                sorted = healthy.OrderBy(p => GetLatencyScore(p)).ToList();
                break;

            case VKAIRoutingStrategy.Preference:
            default:
                sorted = healthy.OrderByDescending(p => GetPreferenceScore(args, p)).ToList();
                break;
        }

        return Task.FromResult(VKResult.Success(sorted));
    }

    private static int GetPreferenceScore(VKAIRouteArgs args, VKAIConnection p)
    {
        int score = 0;
        if (p.IsDefault)
        {
            score += 10;
        }
        if (args.PreferredProvider.HasValue && p.Provider.HasValue && p.Provider.Value == args.PreferredProvider.Value)
        {
            score += 100;
        }
        if (!string.IsNullOrEmpty(args.PreferredModelId) && !string.IsNullOrEmpty(p.ModelId) && string.Equals(p.ModelId, args.PreferredModelId, StringComparison.OrdinalIgnoreCase))
        {
            score += 50;
        }
        return score;
    }

    private static double GetCostScore(VKAIConnection p)
    {
        string model = p.ModelId ?? string.Empty;
        if (model.Contains("mini", StringComparison.OrdinalIgnoreCase) || model.Contains("haiku", StringComparison.OrdinalIgnoreCase) || model.Contains("flash", StringComparison.OrdinalIgnoreCase))
        {
            return 1.0;
        }
        if (model.Contains("gpt-4o", StringComparison.OrdinalIgnoreCase) || model.Contains("sonnet", StringComparison.OrdinalIgnoreCase))
        {
            return 10.0;
        }
        return 5.0;
    }

    private static int GetWeightedScore(VKAIConnection p)
    {
        return p.MaxConcurrency > 0 ? p.MaxConcurrency : 10;
    }

    private double GetLatencyScore(VKAIConnection p)
    {
        if (_metricsCollector is not null)
        {
            double latency = _metricsCollector.GetAverageLatencyMs(p);
            if (latency > 0)
            {
                return latency;
            }
        }

        return p.IsDefault ? 0.0 : 1000.0;
    }
}
