namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Service contract for calculating token costs for AI models.
/// </summary>
public interface IVKAICostCalculator
{
    /// <summary>
    /// Calculates total USD cost based on token consumption.
    /// </summary>
    VKAICostResult CalculateCost(
        string provider,
        string modelId,
        long inputTokens,
        long outputTokens,
        long cachedTokens = 0,
        long reasoningTokens = 0);
}
