using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Typed snapshot of governance gate results, propagated from the early Governance
/// interceptor to the late Context interceptor within a single pipeline execution.
/// </summary>
public sealed record VKGovernanceSnapshot // [AP.01]
{
    /// <summary>
    /// Gets the resolved tenant identifier.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// Gets the resolved user identifier.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Gets the active presence state snapshot.
    /// </summary>
    public required VKPresenceState State { get; init; }

    /// <summary>
    /// Gets the applicable corporate constitution for the tenant.
    /// </summary>
    public required string Constitution { get; init; }

    /// <summary>
    /// Gets the active token quota constraints.
    /// </summary>
    public required VKPresenceQuota Quota { get; init; }
}
