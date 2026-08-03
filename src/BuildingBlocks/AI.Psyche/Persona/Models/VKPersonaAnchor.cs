using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Represents an AI persona anchor. Implements <see cref="IVKTenantScoped"/>.
/// Order follows TenantId -> Id hierarchy with required TenantId.
/// </summary>
public sealed record VKPersonaAnchor : IVKFragmentMetadata, IVKTenantScoped
{
    /// <summary>
    /// Gets the tenant identifier for multi-tenant SaaS isolation.
    /// </summary>
    public required VKTenantId TenantId { get; init; }

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

    /// <summary>
    /// Factory method to create a new <see cref="VKPersonaAnchor"/> with automatic <see cref="IVKIdentityContext"/> resolution.
    /// </summary>
    public static VKPersonaAnchor Create(
        IVKIdentityContext identityContext,
        VKPersonaId id,
        string name,
        string description,
        IReadOnlyDictionary<string, string>? traits = null,
        string? directiveId = null,
        IReadOnlyDictionary<string, object>? extensions = null)
    {
        VKGuard.NotNull(identityContext);
        VKGuard.NotNull(name);
        VKGuard.NotNull(description);

        return new VKPersonaAnchor
        {
            TenantId = identityContext.TenantId,
            Id = id,
            Name = name,
            Description = description,
            Traits = traits ?? new Dictionary<string, string>(),
            DirectiveId = directiveId,
            Extensions = extensions ?? new Dictionary<string, object>()
        };
    }
}
