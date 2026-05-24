namespace VK.Blocks.AI;

/// <summary>
/// Defines the strategy used to route requests when multiple AI engines are available.
/// </summary>
public enum VKRoutingPolicy
{
    /// <summary>
    /// Always attempts the primary engine first. If it fails, falls back to the secondary engines in order.
    /// </summary>
    Priority = 0,

    /// <summary>
    /// Distributes requests evenly across all available engines to balance the load.
    /// </summary>
    RoundRobin = 1,

    /// <summary>
    /// Selects an engine randomly for each request.
    /// </summary>
    Random = 2
}
