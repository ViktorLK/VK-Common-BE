using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents a tenant's Directive containing core system prompt instructions and safety rules.
/// Follows AP.01 (sealed record for immutability). Implements <see cref="IVKTenantScoped"/>.
/// Order follows TenantId -> Id hierarchy with required TenantId.
/// </summary>
public sealed record VKDirectiveCharter : IVKFragmentMetadata, IVKTenantScoped
{
    /// <summary>
    /// Gets the tenant identifier for multi-tenant SaaS isolation.
    /// </summary>
    public required VKTenantId TenantId { get; init; }

    /// <summary>
    /// Gets the directive identifier.
    /// </summary>
    public required VKDirectiveId Id { get; init; }

    /// <summary>
    /// Gets the high-level overview or core system instructions for this directive.
    /// </summary>
    public string? Overview { get; init; }

    /// <summary>
    /// Gets the behavioral guidelines and principles for AI interaction.
    /// </summary>
    public string? BehaviorRules { get; init; }

    /// <summary>
    /// Gets the safety protocols and refusal policies to prevent harmful outputs.
    /// </summary>
    public string? SafetyRules { get; init; }

    /// <summary>
    /// Gets the formatting and structural output constraints (e.g. Markdown, JSON schema).
    /// </summary>
    public string? OutputConstraints { get; init; }

}
