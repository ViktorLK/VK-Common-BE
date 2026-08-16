using System;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Persona.Entities;

/// <summary>
/// Database entity representing a VK AI Persona.
/// </summary>
public sealed class PwpPersonaEntity : IVKMultiTenantEntity, IVKAuditable
{
    public VKTenantId? TenantId { get; set; }
    public required VKPersonaId Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Personality { get; set; }
    public string? Scenario { get; set; }
    public string? FirstMessage { get; set; }
    public string? DialogueExamples { get; set; }
    public string? Traits { get; set; }
    public VKDirectiveId? DirectiveId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
