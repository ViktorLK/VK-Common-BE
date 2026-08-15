namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Pricing configuration rate model for a specific AI model.
/// </summary>
public sealed record VKModelPricing
{
    public required string Provider { get; init; }
    public required string ModelId { get; init; }
    public double CostPer1KPromptTokens { get; init; }
    public double CostPer1KCompletionTokens { get; init; }
}
