namespace VK.Blocks.AI.Engram;

/// <summary>
/// Directive emitted by scoring strategies to control memory lifecycle routing.
/// </summary>
public enum VKScoringDirective
{
    /// <summary>
    /// Normal scoring evaluation. Proceed with base importance scoring.
    /// </summary>
    Score,

    /// <summary>
    /// Fact routed to IVKMemoryStructured KV store. Removed from ordinary memory lifecycle.
    /// </summary>
    RouteToStructured,

    /// <summary>
    /// Security rejection (e.g. password, private key). Immediately discarded and deleted.
    /// </summary>
    SecurityReject
}
