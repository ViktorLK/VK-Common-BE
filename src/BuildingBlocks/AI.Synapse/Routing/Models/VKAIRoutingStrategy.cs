namespace VK.Blocks.AI.Synapse;

/// <summary>
/// Strategy used by the AI Router to order and select candidate providers.
/// </summary>
public enum VKAIRoutingStrategy : byte
{
    /// <summary>
    /// Orders candidates by preference and priority.
    /// </summary>
    Priority = 0,

    /// <summary>
    /// Distributes candidates using weighted ordering based on capacity settings.
    /// </summary>
    Weighted = 1,

    /// <summary>
    /// Distributes candidates evenly in round-robin order.
    /// </summary>
    RoundRobin = 2,

    /// <summary>
    /// Pins requests to a consistent candidate using a deterministic hash of the session affinity key.
    /// </summary>
    SessionAffinity = 3,

    /// <summary>
    /// Orders candidates by lowest estimated cost using price source data.
    /// </summary>
    LowestCost = 4
}
