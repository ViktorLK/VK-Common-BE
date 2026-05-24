namespace VK.Blocks.AI.Cognitive;

/// <summary>
/// Represents a composite key for looking up or saving presence states,
/// providing strict security boundaries across tenants and users.
/// Follows AP.01 (sealed record with required properties) and AP.03.
/// </summary>
public sealed record VKPresenceStoreKey
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// Gets the user identifier.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    public required string SessionId { get; init; }
}
