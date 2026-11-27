using System;
using System.Linq;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Cost.Internal;

// [AP.01] sealed
internal sealed class ConfigurationAIPriceSource : IVKAIPriceSource
{
    private readonly VKCostOptions _options;
    private readonly TimeProvider _timeProvider;

    public ConfigurationAIPriceSource(VKCostOptions options, TimeProvider timeProvider)
    {
        _options = VKGuard.NotNull(options);
        _timeProvider = VKGuard.NotNull(timeProvider);
    }

    public VKModelPricing? GetPricing(string provider, string modelId, DateTimeOffset? asOf = null)
    {
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(modelId) || _options.CustomPricing is null)
        {
            return null;
        }

        var effectiveAt = asOf ?? _timeProvider.GetUtcNow();

        return _options.CustomPricing
            .Where(p => string.Equals(p.Provider, provider, StringComparison.OrdinalIgnoreCase)
                && string.Equals(p.ModelId, modelId, StringComparison.OrdinalIgnoreCase)
                && (p.EffectiveFrom is null || p.EffectiveFrom.Value <= effectiveAt))
            .OrderByDescending(p => p.EffectiveFrom ?? DateTimeOffset.MinValue)
            .FirstOrDefault();
    }
}
