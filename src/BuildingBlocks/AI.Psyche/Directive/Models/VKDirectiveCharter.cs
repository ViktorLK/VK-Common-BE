namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a tenant's Directive containing core system prompt instructions and safety rules.
/// Follows AP.01 (sealed record for immutability).
/// </summary>
public sealed record VKDirectiveCharter : IVKFragmentMetadata
{
    /// <summary>
    /// Gets the directive identifier.
    /// </summary>
    public required VKDirectiveId Id { get; init; }

    public string? BehaviorRules { get; init; }
    public string? SafetyRules { get; init; }
    public string? OutputConstraints { get; init; }
    public string? Overview { get; init; }
}
