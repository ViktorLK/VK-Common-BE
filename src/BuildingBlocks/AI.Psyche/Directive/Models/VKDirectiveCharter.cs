using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a tenant's Directive containing core system prompt instructions and safety rules.
/// Follows AP.01 (sealed record for immutability). Implements <see cref="IVKTenantScoped"/>.
/// Order follows TenantId -> Id hierarchy.
/// </summary>
public sealed record VKDirectiveCharter : IVKFragmentMetadata, IVKTenantScoped
{
    /// <summary>
    /// Gets the tenant identifier for multi-tenant SaaS isolation. Defaults to <see cref="VKTenantId.Default"/>.
    /// </summary>
    public VKTenantId TenantId { get; init; } = VKTenantId.Default;

    /// <summary>
    /// Gets the directive identifier.
    /// </summary>
    public required VKDirectiveId Id { get; init; }

    public string? BehaviorRules { get; init; }
    public string? SafetyRules { get; init; }
    public string? OutputConstraints { get; init; }
    public string? Overview { get; init; }
}
