using System.Collections.Generic;

namespace VK.Blocks.AI.Tokenics.Costing.Internal;

/// <summary>
/// Default implementation of <see cref="IVKTokenCostCalculator"/>.
/// Uses a basic rate map for common models.
/// </summary>
internal sealed class DefaultTokenCostCalculator : IVKTokenCostCalculator
{
    // Rates per 1M tokens (Heuristic rates in USD)
    private static readonly Dictionary<string, (decimal Input, decimal Output)> ModelRates = new()
    {
        { "gpt-4o", (5.0m, 15.0m) },
        { "gpt-4-turbo", (10.0m, 30.0m) },
        { "gpt-3.5-turbo", (0.5m, 1.5m) },
        { "claude-3-opus", (15.0m, 75.0m) },
        { "claude-3-sonnet", (3.0m, 15.0m) },
        { "deepseek-chat", (0.1m, 0.2m) }
    };

    /// <inheritdoc />
    public VKTokenCost CalculateCost(VKAITokenUsage usage, string modelId)
    {
        if (string.IsNullOrEmpty(modelId))
        {
            return new VKTokenCost(0, "USD", 0, 0);
        }

        // Basic lookup with fallback to zero
        var normalizedModelId = modelId.ToLowerInvariant();
        var rates = (Input: 0.0m, Output: 0.0m);

        foreach (var entry in ModelRates)
        {
            if (normalizedModelId.Contains(entry.Key))
            {
                rates = entry.Value;
                break;
            }
        }

        decimal inputCost = (usage.InputTokens / 1_000_000.0m) * rates.Input;
        decimal outputCost = (usage.OutputTokens / 1_000_000.0m) * rates.Output;

        return new VKTokenCost(inputCost + outputCost, "USD", inputCost, outputCost);
    }
}
