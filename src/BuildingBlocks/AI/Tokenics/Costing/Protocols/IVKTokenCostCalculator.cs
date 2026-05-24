namespace VK.Blocks.AI;

/// <summary>
/// Defines the contract for an AI Token Cost Calculator.
/// Calculates the monetary cost of AI operations based on token usage.
/// </summary>
public interface IVKTokenCostCalculator
{
    /// <summary>
    /// Calculates the estimated cost for a given token usage and model.
    /// </summary>
    /// <param name="usage">The token usage (input/output).</param>
    /// <param name="modelId">The model ID.</param>
    /// <returns>A <see cref="VKTokenCost"/> result.</returns>
    VKTokenCost CalculateCost(VKAITokenUsage usage, string modelId);
}

/// <summary>
/// Represents the calculated cost of an AI operation.
/// </summary>
public record VKTokenCost(decimal TotalCost, string Currency, decimal InputCost, decimal OutputCost);
