using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Synapse.Cost.Internal;

// [AP.01] sealed
internal sealed class DefaultAICostCalculator : IVKAICostCalculator
{
    private readonly ConcurrentDictionary<string, VKModelPricing> _pricingMatrix = new(StringComparer.OrdinalIgnoreCase);
    private readonly VKCostOptions _options;

    public DefaultAICostCalculator(VKCostOptions options)
    {
        _options = VKGuard.NotNull(options);

        // Preload standard baseline pricing with strong-typed VKAIModelIds
        AddDefaultPricing("OpenAI", VKAIModelIds.OpenAI.Gpt4O, 0.005, 0.015);
        AddDefaultPricing("OpenAI", VKAIModelIds.OpenAI.Gpt4OMini, 0.00015, 0.0006);
        AddDefaultPricing("Anthropic", VKAIModelIds.Anthropic.Claude35Sonnet, 0.003, 0.015);
        AddDefaultPricing("Anthropic", VKAIModelIds.Anthropic.Claude35Haiku, 0.0008, 0.004);
        AddDefaultPricing("Google", VKAIModelIds.Google.Gemini20Flash, 0.0001, 0.0004);
        AddDefaultPricing("Google", VKAIModelIds.Google.Gemini15Pro, 0.00125, 0.005);

        // Apply any custom pricing overrides configured via options
        if (_options.CustomPricing != null)
        {
            foreach (var pricing in _options.CustomPricing)
            {
                var key = $"{pricing.Provider}:{pricing.ModelId}";
                _pricingMatrix[key] = pricing;
            }
        }
    }

    public double CalculateCost(string provider, string modelId, long promptTokens, long completionTokens)
    {
        if (!_options.Enabled)
        {
            return 0.0;
        }

        var key = $"{provider}:{modelId}";
        if (_pricingMatrix.TryGetValue(key, out var pricing))
        {
            double promptCost = (promptTokens / 1000.0) * pricing.CostPer1KPromptTokens;
            double completionCost = (completionTokens / 1000.0) * pricing.CostPer1KCompletionTokens;
            return promptCost + completionCost;
        }

        // Fallback generic estimate ($0.001 / 1k tokens)
        return ((promptTokens + completionTokens) / 1000.0) * 0.001;
    }

    private void AddDefaultPricing(string provider, string modelId, double promptPer1K, double completionPer1K)
    {
        var key = $"{provider}:{modelId}";
        _pricingMatrix[key] = new VKModelPricing
        {
            Provider = provider,
            ModelId = modelId,
            CostPer1KPromptTokens = promptPer1K,
            CostPer1KCompletionTokens = completionPer1K
        };
    }
}
