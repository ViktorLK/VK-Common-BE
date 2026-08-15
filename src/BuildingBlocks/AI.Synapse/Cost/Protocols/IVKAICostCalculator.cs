namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Service contract for calculating token costs for AI models.
/// </summary>
public interface IVKAICostCalculator
{
    /// <summary>
    /// Calculates total USD cost based on token consumption.
    /// </summary>
    double CalculateCost(string provider, string modelId, long promptTokens, long completionTokens);
}
