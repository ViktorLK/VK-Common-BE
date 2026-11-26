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
    private readonly IVKAIPriceSource _priceSource;
    private readonly IVKAIMetricsCollector _metricsCollector;
    private int _roundRobinCounter;

    public DefaultAIRouter(
        IVKAIProviderTracker tracker,
        VKRoutingOptions options,
        IVKAIPriceSource priceSource,
        IVKAIMetricsCollector metricsCollector)
    {
        _tracker = VKGuard.NotNull(tracker);
        _options = VKGuard.NotNull(options);
        _priceSource = VKGuard.NotNull(priceSource);
        _metricsCollector = VKGuard.NotNull(metricsCollector);
    }

    public Task<VKResult<IReadOnlyList<VKAIConnection>>> ResolveCandidatesAsync(
        VKAIRouteArgs? args,
        IEnumerable<VKAIConnection> pool,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        args ??= new VKAIRouteArgs();
        VKGuard.NotNull(pool);

        var candidateList = pool.ToList();
        if (candidateList.Count == 0)
        {
            return Task.FromResult(VKResult.Failure<IReadOnlyList<VKAIConnection>>(VKAISynapseErrors.NoAvailableProvider));
        }

        // 1. Filter by Enabled, Region, RequiredTags, and health (_tracker.IsAvailable)
        var healthy = candidateList
            .Where(p => p.Enabled
                && MatchesRegion(args, p)
                && MatchesTags(args, p)
                && _tracker.IsAvailable(p))
            .ToList();

        if (healthy.Count == 0)
        {
            return Task.FromResult(VKResult.Failure<IReadOnlyList<VKAIConnection>>(VKAISynapseErrors.NoAvailableProvider));
        }

        // 2. Order based on configured strategy
        List<VKAIConnection> sorted = _options.Strategy switch
        {
            VKAIRoutingStrategy.Weighted => healthy.OrderByDescending(GetWeightedScore).ToList(),
            VKAIRoutingStrategy.RoundRobin => Rotate(healthy, (uint)Interlocked.Increment(ref _roundRobinCounter)),
            VKAIRoutingStrategy.SessionAffinity => !string.IsNullOrEmpty(args.AffinityKey)
                ? Rotate(healthy, ComputeDeterministicHash(args.AffinityKey))
                : OrderByPriority(args, healthy),
            VKAIRoutingStrategy.LowestCost => healthy.OrderBy(GetEstimatedCostScore).ToList(),
            VKAIRoutingStrategy.Priority or _ => OrderByPriority(args, healthy)
        };

        // 3. Apply explicit PreferredConnectionId override if present
        if (!string.IsNullOrEmpty(args.PreferredConnectionId))
        {
            var preferred = sorted.FirstOrDefault(p =>
                string.Equals(p.Id, args.PreferredConnectionId, StringComparison.OrdinalIgnoreCase));

            if (preferred is not null)
            {
                var reordered = new List<VKAIConnection>(sorted.Count) { preferred };
                foreach (var item in sorted)
                {
                    if (!ReferenceEquals(item, preferred))
                    {
                        reordered.Add(item);
                    }
                }

                sorted = reordered;
            }
        }

        return Task.FromResult(VKResult.Success<IReadOnlyList<VKAIConnection>>(sorted));
    }

    private static bool MatchesRegion(VKAIRouteArgs args, VKAIConnection connection)
    {
        if (string.IsNullOrEmpty(args.Region))
        {
            return true;
        }

        return string.Equals(connection.Region, args.Region, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesTags(VKAIRouteArgs args, VKAIConnection connection)
    {
        if (args.RequiredTags is null || args.RequiredTags.Count == 0)
        {
            return true;
        }

        foreach (var required in args.RequiredTags)
        {
            if (!connection.Tags.TryGetValue(required.Key, out var value)
                || !string.Equals(value, required.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static int GetPriorityScore(VKAIRouteArgs args, VKAIConnection p)
    {
        int score = 0;
        if (args.PreferredProvider.HasValue && p.Provider.HasValue && p.Provider.Value == args.PreferredProvider.Value)
        {
            score += 100;
        }
        if (!string.IsNullOrEmpty(args.PreferredModelId) && !string.IsNullOrEmpty(p.ModelId)
            && string.Equals(p.ModelId, args.PreferredModelId, StringComparison.OrdinalIgnoreCase))
        {
            score += 50;
        }
        return score;
    }

    private List<VKAIConnection> OrderByPriority(VKAIRouteArgs args, List<VKAIConnection> healthy)
    {
        return _options.PreferLowestLatency
            ? healthy.OrderByDescending(p => GetPriorityScore(args, p)).ThenBy(p => _metricsCollector.GetAverageLatencyMs(p)).ToList()
            : healthy.OrderByDescending(p => GetPriorityScore(args, p)).ToList();
    }

    private decimal GetEstimatedCostScore(VKAIConnection p)
    {
        var provider = (p.Provider ?? VKAIProviderType.OpenAI).ToString();
        var modelId = p.ModelId ?? VKAIModelIds.OpenAI.Gpt6Luna;
        var pricing = _priceSource.GetPricing(provider, modelId);
        return pricing is not null ? pricing.CostPer1KInputTokens + pricing.CostPer1KOutputTokens : decimal.MaxValue;
    }

    private static int GetWeightedScore(VKAIConnection p)
    {
        return p.MaxConcurrency > 0 ? p.MaxConcurrency : 10;
    }

    private static List<VKAIConnection> Rotate(List<VKAIConnection> items, uint offset)
    {
        if (items.Count <= 1)
        {
            return items;
        }

        int start = (int)(offset % (uint)items.Count);
        if (start == 0)
        {
            return items;
        }

        var rotated = new List<VKAIConnection>(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            rotated.Add(items[(start + i) % items.Count]);
        }

        return rotated;
    }

    private static uint ComputeDeterministicHash(string key)
    {
        unchecked
        {
            uint hash = 2166136261u;
            foreach (char c in key)
            {
                hash ^= c;
                hash *= 16777619u;
            }

            return hash;
        }
    }
}
