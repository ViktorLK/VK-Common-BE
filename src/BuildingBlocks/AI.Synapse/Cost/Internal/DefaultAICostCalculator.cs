using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Cost.Internal;

// [AP.01] sealed
internal sealed class DefaultAICostCalculator : IVKAICostCalculator
{
    private readonly IVKAIPriceSource _priceSource;
    private readonly VKCostOptions _options;
    private readonly TimeProvider _timeProvider;

    public DefaultAICostCalculator(
        IVKAIPriceSource priceSource,
        VKCostOptions options,
        TimeProvider timeProvider)
    {
        _priceSource = VKGuard.NotNull(priceSource);
        _options = VKGuard.NotNull(options);
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    public VKAICostResult CalculateCost(
        string provider,
        string modelId,
        long inputTokens,
        long outputTokens,
        long cachedTokens = 0,
        long reasoningTokens = 0)
    {
        if (!_options.Enabled)
        {
            return VKAICostResult.Unpriced;
        }

        var pricing = _priceSource.GetPricing(provider, modelId, _timeProvider.GetUtcNow());
        if (pricing is null)
        {
            return VKAICostResult.Unpriced;
        }

        decimal inputCost = (inputTokens / 1000m) * pricing.CostPer1KInputTokens;
        decimal outputCost = (outputTokens / 1000m) * pricing.CostPer1KOutputTokens;
        decimal cachedCost = (cachedTokens / 1000m) * pricing.CostPer1KCachedTokens;
        decimal reasoningCost = (reasoningTokens / 1000m) * pricing.CostPer1KReasoningTokens;

        return VKAICostResult.Priced(inputCost + outputCost + cachedCost + reasoningCost);
    }
}
