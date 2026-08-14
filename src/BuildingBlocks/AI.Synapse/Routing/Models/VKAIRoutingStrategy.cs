namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Strategy used by the AI Router to order and select candidate providers.
/// </summary>
public enum VKAIRoutingStrategy
{
    /// <summary>
    /// Orders candidates strictly by user preferred provider and preferred model.
    /// </summary>
    Preference = 0,

    /// <summary>
    /// Distributes candidates using weighted round-robin based on provider weight settings.
    /// </summary>
    WeightedRoundRobin = 1,

    /// <summary>
    /// Prioritizes candidates with lowest estimated token cost.
    /// </summary>
    CostOptimized = 2,

    /// <summary>
    /// Prioritizes candidates with lowest recorded latency.
    /// </summary>
    LatencyOptimized = 3
}
