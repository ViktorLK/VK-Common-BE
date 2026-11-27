using System;

namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Pricing configuration rate model for a specific AI model.
/// </summary>
public sealed record VKModelPricing
{
    public required string Provider { get; init; }
    public required string ModelId { get; init; }
    public decimal CostPer1KInputTokens { get; init; }
    public decimal CostPer1KOutputTokens { get; init; }
    public decimal CostPer1KCachedTokens { get; init; }
    public decimal CostPer1KReasoningTokens { get; init; }
    public DateTimeOffset? EffectiveFrom { get; init; }
}
