namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Immutable snapshot of cumulative token and cost usage for a given scope.
/// </summary>
// [AP.01]
public sealed record VKCumulativeQuotaUsage
{
    public long TotalTokens { get; init; }
    public decimal TotalCost { get; init; }
}
