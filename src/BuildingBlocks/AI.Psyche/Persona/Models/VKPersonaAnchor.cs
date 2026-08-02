using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents an AI persona anchor. Implements <see cref="IVKTenantScoped"/>.
/// Order follows TenantId -> Id hierarchy.
/// </summary>
public sealed record VKPersonaAnchor : IVKFragmentMetadata, IVKTenantScoped
{
    /// <summary>
    /// Gets the tenant identifier for multi-tenant SaaS isolation. Defaults to <see cref="VKTenantId.Default"/>.
    /// </summary>
    public VKTenantId TenantId { get; init; } = VKTenantId.Default;

    /// <summary>
    /// Gets the unique identifier for the persona.
    /// </summary>
    public required VKPersonaId Id { get; init; }

    /// <summary>
    /// Gets the name of the persona.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the core description of the persona.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets specific personality traits and behavioral principles of the persona.
    /// Used for industrial definitions (e.g. Tone: Professional, Format: JSON).
    /// </summary>
    public IReadOnlyDictionary<string, string> Traits { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets the ID of the specific Directive Charter to use for this persona.
    /// Overrides the tenant default if specified.
    /// </summary>
    public string? DirectiveId { get; init; }

    /// <summary>
    /// Gets custom unstructured properties allowing downstream extensions (e.g. for PWP).
    /// </summary>
    public IReadOnlyDictionary<string, object> Extensions { get; init; } = new Dictionary<string, object>();
}
